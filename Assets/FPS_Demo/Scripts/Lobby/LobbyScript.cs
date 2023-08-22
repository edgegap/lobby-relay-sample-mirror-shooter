using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using Edgegap;
using static CustomEvents;

public class LobbyScript : NetworkBehaviour
{
    private readonly HttpClient _httpClient = new();

    public EdgegapTransport _EdgegapTransport = EdgegapTransport.GetInstance();

    public static string lobbyApiUrl = "https://abc-cb74ab23450fd5.edgegap.net"; //subject to change, make sure its up to date

    /// <summary>
    /// Get List
    /// </summary>
    /// <returns>List of lobbies</returns>
    public async Task<LobbiesResponse> RequestGet()
    {
        var response = await _httpClient.GetAsync($"{lobbyApiUrl}/lobbies");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Error code: {response.StatusCode}");
        }

        string responseContent = await response.Content.ReadAsStringAsync();
        LobbiesResponse content = JsonConvert.DeserializeObject<LobbiesResponse>(responseContent);

        return content;
    }

    /// <summary>
    /// Get Lobbby
    /// </summary>
    /// <param name="lobbyId">Id of lobby</param>
    /// <returns>Lobby</returns>
    public async Task<Lobby> RequestGet(string lobbyId)
    {
        var response = await _httpClient.GetAsync($"{lobbyApiUrl}/lobbies/{lobbyId}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Error code: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        Lobby content = JsonConvert.DeserializeObject<Lobby>(responseContent);

        return content;
    }

    /// <summary>
    /// Create new Lobby
    /// </summary>
    /// <param name="capacity">How many players can join lobby</param>
    /// <param name="name">Name of lobby</param>
    /// <param name="player">Player who created lobby</param>
    /// <param name="annotations">List of key/value pairs that can be injected</param>
    /// <param name="isJoinable">If lobby can be joined</param>
    /// <param name="tags">List of tags</param>
    /// <returns>Newly created Lobby</returns>
    public async Task<Lobby> RequestPost(int capacity, string name, Player player,
        List<Annotation> annotations = null, bool isJoinable = true, List<string> tags = null)
    {
        CreatePayload objectToSerialize = new()
        {
            capacity = capacity,
            is_joinable = isJoinable,
            name = name,
            player = player
        };
        objectToSerialize.annotations = annotations is not null ? annotations : new List<Annotation>();
        objectToSerialize.tags = tags is not null ? tags : new List<string>();

        var jsonContent = new StringContent(JsonConvert.SerializeObject(objectToSerialize), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{lobbyApiUrl}/lobbies", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Error code: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        Lobby content = JsonConvert.DeserializeObject<Lobby>(responseContent);

        return content;
    }

    /// <summary>
    /// Start Lobby
    /// </summary>
    /// <param name="lobbyId">Id of Lobby</param>
    /// <returns>completed task</returns>
    public async Task RequestPost(string lobbyId)
    {
        StartPayload objectToSerialize = new()
        {
            lobby_id = lobbyId
        };

        var jsonContent = new StringContent(JsonConvert.SerializeObject(objectToSerialize), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{lobbyApiUrl}/lobbies:start", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Error code: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Either Join or Leave Lobby
    /// </summary>
    /// <param name="action">join or leave</param>
    /// <param name="lobbyId">Id of Lobby</param>
    /// <param name="player">Player that joins/leaves the Lobby</param>
    /// <returns>completed task</returns>
    public async Task RequestPost(string action, string lobbyId, Player player)
    {
        JoinOrLeavePayload objectToSerialize = new()
        {
            lobby_id = lobbyId,
            player = player
        };

        var jsonContent = new StringContent(JsonConvert.SerializeObject(objectToSerialize), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{lobbyApiUrl}/lobbies:{action}", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Error code: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Delete Lobby
    /// </summary>
    /// <param name="lobbyId">Id of Lobby</param>
    /// <returns>ccompleted task</returns>
    public async Task RequestDelete(string lobbyId)
    {
        var response = await _httpClient.DeleteAsync($"{lobbyApiUrl}/lobbies/{lobbyId}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Error code: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Connects the player to the started lobby
    /// </summary>
    /// <param name="lobby">Lobby to connect to</param>
    /// <param name="player">Thhe player to connect</param>
    public void StartMatch(Lobby lobby, Player player)
    {
        Assignment assign = lobby.assignment;
        ushort clientPort = (ushort)assign.ports.Find(port => port.name == "client").port;
        ushort serverPort = (ushort)assign.ports.Find(port => port.name == "server").port;

        _EdgegapTransport.ChangeValue(
            assign.ip,
            clientPort,
            serverPort,
            lobby.lobby_id,
            assign.authorization_token,
            (uint)player.authorization_token
            );

        if (player.is_host)
        {
            NetworkManager.singleton.StartHost();
        }
        else
        {
            NetworkManager.singleton.StartClient();
        }
    }


    #region JSON Data Classes

    #region Payloads
    public class CreatePayload
    {
        public List<Annotation> annotations { get; set; }
        public int capacity { get; set; }
        public bool is_joinable { get; set; }
        public string name { get; set; }
        public Player player { get; set; }
        public List<string> tags { get; set; }
    }

    public class JoinOrLeavePayload
    {
        public string lobby_id { get; set; }
        public Player player { get; set; }
    }

    public class StartPayload
    {
        public string lobby_id { get; set; }
    }
    #endregion Payloads

    #region Responses
    public class LobbiesResponse
    {
        public int count { get; set; }
        public List<Lobby> data { get; set; }
    }

    public class Lobby
    {
        public List<Annotation> annotations { get; set; }
        public Assignment assignment { get; set; }
        public int capacity { get; set; }
        public bool is_joinable { get; set; }
        public bool is_started { get; set; }
        public string lobby_id { get; set; }
        public string name { get; set; }
        public int player_count { get; set; }
        public List<Player> players { get; set; }
        public List<string> tags { get; set; }
    }
    #endregion Responses

    public class Annotation
    {
        public bool inject { get; set; }
        public string key { get; set; }
        public string value { get; set; }
    }

    public class Assignment
    {
        public uint authorization_token { get; set; }
        public string host { get; set; }
        public string ip { get; set; }
        public List<Port> ports { get; set; }
    }

    public class Port
    {
        public string name { get; set; }
        public int port { get; set; }
        public string protocol { get; set; }
    }

    public class Player
    {
        public uint? authorization_token { get; set; }
        public string id { get; set; }
        public bool is_host { get; set; }
    }
    #endregion DataClasses
}
