using Sandbox;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
[Title( "SMT" )]
[Category( "Networking" )]
[Icon( "electrical_services" )]
public sealed class Server : Component, Component.INetworkListener
{
    [Property] public bool StartServer;
    [Property] public GameObject PlayerPrefab;
    [Property] public List<GameObject> Players;
    [System.Serializable]
    public class CustomIntVariable
    {
        [Property] public string Name { get; set; }
        [Property] public int Value { get; set; }
    }

    [Property] public List<CustomIntVariable> CustomVariables { get; set; } = new();
    [Property] public bool DoesNeedTimer;
    [Property] public bool DoesPlayersNeedHealth;
    [Property] public bool DoesPlayersNeedMoney;
    [Property] public float time;
    [Property] public float GameTime;
    [Property] public GameObject FirstSpawnPoint;
    [Property] public float RelaxTime;
    [Property] public bool IsGameRunning;
    [Property] public List<GameObject> SpawnPoints;
    [Property] public int MinPlayersNeed;
    [Property] public bool DebugMode;
    [Property] public List<Action> CustomAction;
    [Property] public Action OnGameStarted { get; set; }
    [Property] public Action OnGameStopped { get; set; }
    protected override void OnStart()
    {
        base.OnStart();
    }
    protected override void OnUpdate()
    {
        if ( DoesNeedTimer )
        {
            if ( time >= 0 )
            {
                time -= Time.Delta;
            }
            else
            {
                if ( DebugMode )
                {
                    if ( IsGameRunning )
                    {
                        Log.Info( "Game time is over, stopping game..." );
                        StopGame();
                    }
                    else
                    {
                        if ( !IsProxy )
                        {
                            Log.Info( "Enough players, starting game..." );
                            StartGame();
                        }
                    }
                }
                else
                {
                    if ( IsGameRunning )
                    {
                        Log.Info( "Game time is over, stopping game..." );
                        StopGame();
                    }
                    else
                    {
                        if ( !IsProxy && Players.Count >= MinPlayersNeed )
                        {
                            Log.Info( "Enough players, starting game..." );
                            StartGame();
                        }
                        else
                        {
                            Log.Info( $"Waiting for players... ({Players.Count}/{MinPlayersNeed})" );
                            time = 5f;
                        }
                    }
                }
            }
        }
    }
    [Rpc.Broadcast]
    public void StartGame()
    {
        OnGameStarted.Invoke();
        foreach ( var player in Players )
        {
            //player.GetComponent<Player>().Teleport( GameSpawnPoint.GetComponent<Transform>().Position );
            if ( DoesPlayersNeedHealth )
            {
                player.GetComponent<Player>().Health = 100;
                player.GetComponent<Player>().IsAlive = true;
            }

        }
        time = GameTime;
        IsGameRunning = true;
        Log.Info( "Game started!" );
    }
    [Rpc.Broadcast]
    public void StopGame()
    {
        OnGameStopped.Invoke();
        foreach ( var player in Players )
        {
            //player.GetComponent<Player>().Teleport( RelaxSpawnPoint.GetComponent<Transform>().Position );
            if ( DoesPlayersNeedHealth )
            {
                player.GetComponent<Player>().Health = 100;
                player.GetComponent<Player>().IsAlive = true;
            }

        }
        time = RelaxTime;
        IsGameRunning = false;
        Log.Info( "Game stopped!" );
    }
    protected override async Task OnLoad()
	{
		if ( Scene.IsEditor )
			return;

		if ( StartServer && !Networking.IsActive )
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds( 0.1f );
			Networking.CreateLobby( new() );
		}
	}
    public void OnActive(Connection channel)
    {
        Log.Info( $"Player '{channel.Name}' has joined the game" );
        if ( !PlayerPrefab.IsValid() )
			return;
        var player = PlayerPrefab.Clone( FirstSpawnPoint.GetComponent<Transform>(), name: $"Player - {channel.Name}" );
        player.GetComponentInChildren<TextRenderer>().Text = channel.Name;
        player.NetworkSpawn( channel );
        Players.Add( player );
    }
}