﻿using Muuzika.Server.Dtos.Gateway;
using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Extensions.Room;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Models;

namespace Muuzika.Server.Mappers;

public class RoomMapper: IRoomMapper
{
    private readonly IPlayerMapper _playerMapper;
    
    public RoomMapper(IPlayerMapper playerMapper)
    {
        _playerMapper = playerMapper;
    }

    public RoomDto ToDto(Room room)
    {
        return new RoomDto(
            Code: room.Code,
            LeaderUsername: room.Leader.Username,
            Status: room.Status,
            Players: room.GetPlayers().Select(_playerMapper.ToDto),
            Options: room.Options
        );
    }

    public RoomCreatedOrJoinedDto ToCreatedOrJoinedDto(Room room, Player player)
    {
        return new RoomCreatedOrJoinedDto(
            Username: player.Username,
            RoomCode: room.Code,
            Token: room.GetTokenForPlayer(player)
        );
    }
    
    public StateSyncDto ToStateSyncDto(Room room, Player player)
    {
        return new StateSyncDto(
            Room: ToDto(room),
            Player: _playerMapper.ToDto(player)
        );
    }
}