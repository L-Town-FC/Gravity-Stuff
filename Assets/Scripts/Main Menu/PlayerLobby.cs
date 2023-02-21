using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerLobby : MonoBehaviour
{
    //video where I copied everything from
    //https://www.youtube.com/watch?v=-KDlEBfCBiU&list=PLq00P7T1AsJ8jeM7A3bHWuR1dC0FZNM88&index=9&t=1019s

    //POSSIBLE WAY TO GET ALL CLIENTS TO START GAME
    //Have a field in the lobby called isGameStarted
    //When the host starts the game this gets updated to true
    //Host instantly loads into game
    //Non-hosts will see update through lobby polling and check this field
    //when the see the updated field, then they will also load into game
    //really janky way of doing it and im not sure if multiple lobbies can be supported but whatever


    //alternate method that is probably how you are supposed to do it
    //create host on when lobby is created and client when player joins
    //these will be empty game objects
    //have network handler handle loading next scene
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private string playerName;

    [SerializeField]
    GameObject _text;
    TMP_Text text;

    private void Awake()
    {
    }

    // Start is called before the first frame update
    private async void Start()
    {
        text = _text.GetComponent<TMP_Text>();
        text.text = "Not in Lobby";

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Tehoy" + UnityEngine.Random.Range(10, 99);
        Debug.Log(playerName);
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdate();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0f)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdate()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer <= 0f)
            {
                float lobbyUpdateTimerMax = 5f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;

                Debug.Log(lobby.Data);

                if (lobby.Data["isStarted"].Value == "true")
                {
                    SceneManager.LoadScene("First Map Attempt");
                }
            }
        }
    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "DeathMatch") },
                    {"isStarted", new DataObject(DataObject.VisibilityOptions.Member, "false")}
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby;

            text.text = "Host";

            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>{
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobby()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
            Debug.Log("Joined Lobby by ID");

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByCode( string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;
            Debug.Log("Joined Lobby with code " + lobbyCode);

            PrintPlayers(lobby);
        }catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };

            Lobby quickJoinedLobby = await Lobbies.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            Debug.Log("Joined Lobby Quickly");
            PrintPlayers(quickJoinedLobby);

            text.text = "Joined";
            joinedLobby = quickJoinedLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    public void PrintPlayers(Lobby lobby)
    {
        try
        {
            Debug.Log("Players in Lobby " + lobby.Name);
            foreach(Player player in lobby.Players)
            {
                Debug.Log(player.Id + " " + player.Data["PlayerName"].Value + lobby.Data["GameMode"].Value);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
            }
        };
    }

    public async void DeleteLobby()
    {
        try
        {
            
            await Lobbies.Instance.DeleteLobbyAsync(hostLobby.Id);
            Debug.Log("Lobby " + hostLobby.Id + " was deleted");
        }catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode)}
                }
            });

            joinedLobby = hostLobby;

            PrintPlayers(hostLobby);
        }catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                }
            });
        }catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    //can only be run by host or error thrown
    public async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    //gives host to player two
    public async void MigrateLobbyHost()
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id
            });

            joinedLobby = hostLobby;

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void StartGame()
    {
        if (joinedLobby.Players.Count > 0 && SceneManager.GetActiveScene().name == "Menu")
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"isStarted", new DataObject(DataObject.VisibilityOptions.Member, "true")}
                }
            });

            joinedLobby = hostLobby;
        }
    }


}
