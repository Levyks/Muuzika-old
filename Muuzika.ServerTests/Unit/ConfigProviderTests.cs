﻿using Microsoft.Extensions.Configuration;
using Moq;
using Muuzika.Server.Enums.Room;
using Muuzika.Server.Providers;

namespace Muuzika.ServerTests.Unit;

public class ConfigProviderTests
{
    private Mock<IConfiguration> _configurationMock;

    private const string JwtKey = "JwtKey";
    private const string JwtIssuer = "JwtIssuer";
    private const string JwtAudience = "JwtAudience";
    private const string DelayCloseRoomAfterLastPlayerLeft = "00:00:10";
    private const string DelayDisconnectedPlayerRemoval = "00:00:10";
    private const string RoomDefaultPossibleRoundTypes = "Both";
    private const string RoomDefaultRoundsCount = "10";
    private const string RoomDefaultRoundDuration = "00:00:10";
    private const string RoomDefaultMaxPlayersCount = "10";
    
    [SetUp]
    public void Setup()
    {
        _configurationMock = new Mock<IConfiguration>();
        
        _configurationMock.Setup(x => x["Jwt:Key"]).Returns(JwtKey);
        _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns(JwtIssuer);
        _configurationMock.Setup(x => x["Jwt:Audience"]).Returns(JwtAudience);
        _configurationMock.Setup(x => x["Room:DelayCloseRoomAfterLastPlayerLeft"]).Returns(DelayCloseRoomAfterLastPlayerLeft);
        _configurationMock.Setup(x => x["Room:DelayDisconnectedPlayerRemoval"]).Returns(DelayDisconnectedPlayerRemoval);
        _configurationMock.Setup(x => x["Room:DefaultPossibleRoundTypes"]).Returns(RoomDefaultPossibleRoundTypes);
        _configurationMock.Setup(x => x["Room:DefaultRoundsCount"]).Returns(RoomDefaultRoundsCount);
        _configurationMock.Setup(x => x["Room:DefaultRoundDuration"]).Returns(RoomDefaultRoundDuration);
        _configurationMock.Setup(x => x["Room:DefaultMaxPlayersCount"]).Returns(RoomDefaultMaxPlayersCount);
    }
    
    [Test]
    public void ShouldGetConfigsFromIConfiguration()
    {
        var configProvider = new ConfigProvider(_configurationMock.Object);
        
        Assert.Multiple(() =>
        {
            Assert.That(configProvider.JwtKey, Is.EqualTo(JwtKey));
            Assert.That(configProvider.JwtIssuer, Is.EqualTo(JwtIssuer));
            Assert.That(configProvider.JwtAudience, Is.EqualTo(JwtAudience));
            Assert.That(configProvider.DelayCloseRoomAfterLastPlayerLeft, Is.EqualTo(TimeSpan.Parse(DelayCloseRoomAfterLastPlayerLeft)));
            Assert.That(configProvider.DelayDisconnectedPlayerRemoval, Is.EqualTo(TimeSpan.Parse(DelayDisconnectedPlayerRemoval)));
            Assert.That(configProvider.RoomDefaultPossibleRoundTypes, Is.EqualTo(Enum.Parse<RoomPossibleRoundTypes>(RoomDefaultPossibleRoundTypes)));
            Assert.That(configProvider.RoomDefaultRoundsCount, Is.EqualTo(ushort.Parse(RoomDefaultRoundsCount)));
            Assert.That(configProvider.RoomDefaultRoundDuration, Is.EqualTo(TimeSpan.Parse(RoomDefaultRoundDuration)));
            Assert.That(configProvider.RoomDefaultMaxPlayersCount, Is.EqualTo(ushort.Parse(RoomDefaultMaxPlayersCount)));
        });
    }
    
    [Test]
    [TestCase("Jwt:Key")]
    [TestCase("Jwt:Issuer")]
    [TestCase("Jwt:Audience")]
    [TestCase("Room:DelayCloseRoomAfterLastPlayerLeft")]
    [TestCase("Room:DelayDisconnectedPlayerRemoval")]
    [TestCase("Room:DefaultPossibleRoundTypes")]
    [TestCase("Room:DefaultRoundsCount")]
    [TestCase("Room:DefaultRoundDuration")]
    [TestCase("Room:DefaultMaxPlayersCount")]
    public void ShouldThrowExceptionIfConfigIsMissing(string key)
    {
        _configurationMock.Setup(x => x[key]).Returns((string?)null);
        
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            var _ = new ConfigProvider(_configurationMock.Object);
        });
        
            
        Assert.That(exception, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(exception?.ParamName, Is.EqualTo(key));
            Assert.That(exception?.Message, Is.EqualTo($"{key} is not set in configuration (Parameter '{key}')"));
        });
    }

    [Test]
    [TestCase("Room:DelayCloseRoomAfterLastPlayerLeft")]
    [TestCase("Room:DelayDisconnectedPlayerRemoval")]
    [TestCase("Room:DefaultRoundDuration")]
    public void ShouldThrowIfTimeSpanValueIsInvalid(string key)
    {
        const string value = "invalid";
        _configurationMock.Setup(x => x[key]).Returns(value);
        
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            var _ = new ConfigProvider(_configurationMock.Object);
        });
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception?.Message, Is.EqualTo($"{key} is not a valid TimeSpan (value: \"{value}\")"));
    }
    
    [Test]
    [TestCase("Room:DefaultRoundsCount")]
    public void ShouldThrowIfUshortValueIsInvalid(string key)
    {
        const string value = "invalid";
        _configurationMock.Setup(x => x[key]).Returns(value);
        
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            var _ = new ConfigProvider(_configurationMock.Object);
        });
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception?.Message, Is.EqualTo($"{key} is not a valid ushort (value: \"{value}\")"));
    }
    
    [Test]
    [TestCase("Room:DefaultPossibleRoundTypes", typeof(RoomPossibleRoundTypes))]
    public void ShouldThrowIfEnumValueIsInvalid(string key, Type enumType)
    {
        const string value = "invalid";
        _configurationMock.Setup(x => x[key]).Returns(value);
        
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            var _ = new ConfigProvider(_configurationMock.Object);
        });
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception?.Message, Is.EqualTo($"{key} is not a valid value for {enumType.Name} (value: \"{value}\")"));
    }
}