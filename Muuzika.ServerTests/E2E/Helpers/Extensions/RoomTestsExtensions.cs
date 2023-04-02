﻿using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Muuzika.Server.Dtos.Gateway;
using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Enums.Room;

namespace Muuzika.ServerTests.E2E.Helpers.Extensions;

public static class RoomTestsExtensions
{
    private static HubConnection CreateHubConnection(this BaseE2ETest test, string? token)
    {
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(test.Factory.Server.BaseAddress + "hub",  options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => test.Factory.Server.CreateHandler();
                options.Transports = HttpTransportType.ServerSentEvents;
            })
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .Build();
        
        return hubConnection;
    }
    
    public static async Task<(string, HubConnection)> CreateRoomAndConnect(this BaseE2ETest test, string username)
    {
        const string captchaToken = "foo";

        var body = new CreateOrJoinRoomDto(username, captchaToken);
        var response = await test.Client.PostAsJsonAsync("/room", body);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var contentString = await response.Content.ReadAsStringAsync();
        var responseDict = JsonSerializer.Deserialize<Dictionary<string, string>>(contentString);
        
        Assert.Multiple(() =>
        {
            Assert.That(responseDict?.GetValueOrDefault("token"), Is.Not.Null);
            Assert.That(responseDict?.GetValueOrDefault("username"), Is.EqualTo(username));
            Assert.That(responseDict?.GetValueOrDefault("roomCode"), Is.Not.Null);
        });

        var roomCode = responseDict!["roomCode"];

        var hubConnection = test.CreateHubConnection(responseDict["token"]);

        await hubConnection.StartAsync();
        
        var result = await hubConnection.InvokeAsync<InvocationResultDto<StateSyncDto>>("SyncAll");
        
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        
        if (result.Data == null) throw new Exception("Will never happen");
        
        var room = result.Data.Room;
        var player = result.Data.Player;
            
        Assert.Multiple(() =>
        {
            Assert.That(room.Code, Is.EqualTo(roomCode));
            Assert.That(room.Players.Count(), Is.EqualTo(1));
            Assert.That(room.Players.First().Username, Is.EqualTo(username));
            Assert.That(room.LeaderUsername, Is.EqualTo(username));
            Assert.That(room.Status, Is.EqualTo(RoomStatus.InLobby));
                
            Assert.That(player.Username, Is.EqualTo(username));
            Assert.That(player.Score, Is.EqualTo(0));
            Assert.That(player.IsConnected, Is.True);
        });
        
        return (roomCode, hubConnection);
    }
    
    
    public static async Task<HubConnection> JoinRoomAndConnect(this BaseE2ETest test, string roomCode, string username)
    {
        const string captchaToken = "foo";

        var body = new CreateOrJoinRoomDto(username, captchaToken);
        var response = await test.Client.PostAsJsonAsync($"/room/{roomCode}", body);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var contentString = await response.Content.ReadAsStringAsync();
        var responseDict = JsonSerializer.Deserialize<Dictionary<string, string>>(contentString);
        
        Assert.Multiple(() =>
        {
            Assert.That(responseDict?.GetValueOrDefault("token"), Is.Not.Null);
            Assert.That(responseDict?.GetValueOrDefault("username"), Is.EqualTo(username));
            Assert.That(responseDict?.GetValueOrDefault("roomCode"), Is.EqualTo(roomCode));
        });

        var hubConnection = test.CreateHubConnection(responseDict?["token"]);

        await hubConnection.StartAsync();
        
        var result = await hubConnection.InvokeAsync<InvocationResultDto<StateSyncDto>>("SyncAll");
        
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        
        if (result.Data == null) throw new Exception("Will never happen");
        
        var room = result.Data.Room;
        var player = result.Data.Player;
            
        Assert.Multiple(() =>
        {
            Assert.That(room.Code, Is.EqualTo(roomCode));
            Assert.That(room.Status, Is.EqualTo(RoomStatus.InLobby));
            Assert.That(room.Players, Has.Exactly(1).Property("Username").EqualTo(username));
            
            Assert.That(player.Username, Is.EqualTo(username));
            Assert.That(player.Score, Is.EqualTo(0));
            Assert.That(player.IsConnected, Is.True);
        });
        
        return hubConnection;
    }
    
}