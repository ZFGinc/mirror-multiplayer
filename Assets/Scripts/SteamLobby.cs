using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SocialPlatforms.Impl;

[RequireComponent(typeof(NetworkManager))]
public class SteamLobby : MonoBehaviour
{
    [SerializeField] private GameObject _hostButton = null;

    private NetworkManager _networkManager;

    protected Callback<LobbyCreated_t> _onLobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> _onGameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> _onLobbyEntered;

    private const string HostAddressKey = "HostAddress";

    private void Start()
    {
        _networkManager = GetComponent<NetworkManager>();

        if (!SteamManager.Initialized) { return; }

        _onLobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        _onGameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        _onLobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        HostLobby();
    }

    public void HostLobby()
    {
        LeaveLobby();
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, _networkManager.maxConnections);
    }

    public void LeaveLobby()
    {
        if (NetworkServer.active) _networkManager.StopHost();
        else _networkManager.StopClient();
        
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            _hostButton.SetActive(true);
            return;
        }

        _networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());

    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        LeaveLobby();
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) return;

        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

        _networkManager.networkAddress = hostAddress;
        _networkManager.StartClient();
    }
}
