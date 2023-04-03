﻿using Muuzika.Server.Enums.Room;

namespace Muuzika.Server.Providers.Interfaces;

public interface IConfigProvider
{
    string JwtKey { get; }
    string JwtIssuer { get; }
    string JwtAudience { get; }
    
    TimeSpan DelayCloseRoomAfterLastPlayerLeft { get; }
    TimeSpan DelayDisconnectedPlayerRemoval { get; }
    
    RoomPossibleRoundTypes RoomDefaultPossibleRoundTypes { get; }
    ushort RoomDefaultRoundsCount { get; }
    TimeSpan RoomDefaultRoundDuration { get; }
    ushort RoomDefaultMaxPlayersCount { get; }
}