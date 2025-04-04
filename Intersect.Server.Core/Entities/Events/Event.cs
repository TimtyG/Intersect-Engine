using Intersect.Core;
using Intersect.Enums;
using Intersect.Framework.Core;
using Intersect.Framework.Core.GameObjects.Events;
using Intersect.Framework.Core.GameObjects.Events.Commands;
using Intersect.Server.Localization;
using Intersect.Server.Maps;
using Intersect.Server.Networking;
using Intersect.Utilities;
using Microsoft.Extensions.Logging;

namespace Intersect.Server.Entities.Events;


public partial class Event
{

    public EventDescriptor Descriptor;

    public Stack<CommandInstance> CallStack = new Stack<CommandInstance>();

    public bool Global;

    public EventPageInstance[] GlobalPageInstance;

    public bool HoldingPlayer;

    public Guid Id;

    public Guid MapId;

    public Guid MapInstanceId;

    public MapController MapController;

    private Dictionary<string, string> mParams = new Dictionary<string, string>();

    public int PageIndex;

    public EventPageInstance PageInstance;

    public Player Player;

    //Special conditions
    public bool PlayerHasDied;

    public int SpawnX;

    public int SpawnY;

    public long WaitTimer;

    public int X;

    public int Y;

    public Event(Guid instanceId, MapController map, Player player, EventDescriptor eventDescriptor)
    {
        Id = instanceId;
        MapInstanceId = player.MapInstanceId;
        MapId = map?.Id ?? Guid.Empty;
        MapController = map;
        Player = player;
        SelfSwitch = new bool[4];
        Descriptor = eventDescriptor;
        X = eventDescriptor.SpawnX;
        Y = eventDescriptor.SpawnY;
    }

    public Event(Guid instanceId, EventDescriptor eventDescriptor, MapController map, Guid mapInstanceId) //Global constructor
    {
        Id = instanceId;
        MapInstanceId = mapInstanceId;
        Global = true;
        MapId = map?.Id ?? Guid.Empty;
        MapController = map;
        Descriptor = eventDescriptor;
        SelfSwitch = new bool[4];
        GlobalPageInstance = new EventPageInstance[Descriptor.Pages.Count];
        X = (byte) eventDescriptor.SpawnX;
        Y = eventDescriptor.SpawnY;
        for (var i = 0; i < Descriptor.Pages.Count; i++)
        {
            GlobalPageInstance[i] = new EventPageInstance(Descriptor, Descriptor.Pages[i], MapId, mapInstanceId, this, null);
        }
    }

    public bool[] SelfSwitch { get; set; }

    public void Update(long timeMs, MapController map)
    {
        var sendLeave = false;
        var originalPageInstance = PageInstance;
        if (PageInstance != null)
        {
            //Check for despawn
            if (PageInstance.ShouldDespawn(map))
            {
                X = PageInstance.X;
                Y = PageInstance.Y;
                if (PageInstance.GlobalClone != null)
                {
                    Player.GlobalPageInstanceLookup.TryRemove(PageInstance.GlobalClone, out Event val);
                }
                PageInstance = null;
                CallStack.Clear();
                PlayerHasDied = false;
                if (HoldingPlayer)
                {
                    PacketSender.SendReleasePlayer(Player, Id);
                    HoldingPlayer = false;
                }

                sendLeave = true;
            }
            else
            {
                if (!Global)
                {
                    PageInstance.Update(
                        CallStack.Count > 0, timeMs
                    ); //Process movement and stuff that is client specific
                }

                //Check to see if we should process event commands
                if (CallStack.Count > 0)
                {
                    var curStack = CallStack.Peek();
                    if (curStack == null)
                    {
                        ApplicationContext.Context.Value?.Logger.LogError("Curstack variable in event update is null.. not sure how nor how to recover so just gonna let this crash now..");
                    }
                    if (Player == null)
                    {
                        ApplicationContext.Context.Value?.Logger.LogError("Player variable in event update is null.. not sure how nor how to recover so just gonna let this crash now..");
                    }
                    if (curStack.WaitingForResponse == CommandInstance.EventResponse.Shop && Player.InShop == null)
                    {
                        curStack.WaitingForResponse = CommandInstance.EventResponse.None;
                    }

                    if (curStack.WaitingForResponse == CommandInstance.EventResponse.Crafting &&
                        Player.OpenCraftingTableId == Guid.Empty)
                    {
                        curStack.WaitingForResponse = CommandInstance.EventResponse.None;
                    }

                    if (curStack.WaitingForResponse == CommandInstance.EventResponse.Bank && Player.InBank == false)
                    {
                        curStack.WaitingForResponse = CommandInstance.EventResponse.None;
                    }

                    if (curStack.WaitingForResponse == CommandInstance.EventResponse.Quest &&
                        !Player.QuestOffers.Contains(((StartQuestCommand) curStack.WaitingOnCommand).QuestId))
                    {
                        curStack.WaitingForResponse = CommandInstance.EventResponse.None;
                    }

                    if (curStack.WaitingForResponse == CommandInstance.EventResponse.Timer &&
                        WaitTimer < Timing.Global.Milliseconds)
                    {
                        curStack.WaitingForResponse = CommandInstance.EventResponse.None;
                    }

                    if (curStack.WaitingForResponse == CommandInstance.EventResponse.Fade &&
                        !Player.IsFading)
                    {
                        curStack.WaitingForResponse = CommandInstance.EventResponse.None;
                    }

                    var commandsExecuted = 0;
                    while (curStack != null && curStack.WaitingForResponse == CommandInstance.EventResponse.None &&
                           !(PageInstance?.ShouldDespawn(map) ?? false) &&
                           commandsExecuted < Options.Instance.EventWatchdogKillThreshold)
                    {
                        if (curStack.WaitingForRoute != Guid.Empty)
                        {
                            if (curStack.WaitingForRoute == Player.Id)
                            {
                                if (Player.MoveRoute == null ||
                                    Player.MoveRoute.Complete && Player.MoveTimer < Timing.Global.Milliseconds)
                                {
                                    curStack.WaitingForRoute = Guid.Empty;
                                    curStack.WaitingForRouteMap = Guid.Empty;
                                }
                            }
                            else
                            {
                                //Check if the exist exists && if the move route is completed.
                                foreach (var evt in Player.EventLookup)
                                {
                                    if (evt.Value.MapId == curStack.WaitingForRouteMap &&
                                        evt.Value.Descriptor.Id == curStack.WaitingForRoute)
                                    {
                                        if (evt.Value.PageInstance == null)
                                        {
                                            break;
                                        }

                                        if (!evt.Value.PageInstance.MoveRoute.Complete)
                                        {
                                            break;
                                        }

                                        curStack.WaitingForRoute = Guid.Empty;
                                        curStack.WaitingForRouteMap = Guid.Empty;

                                        break;
                                    }
                                }
                            }

                            if (curStack.WaitingForRoute != Guid.Empty)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (curStack.CommandIndex >= curStack.CommandList.Count)
                            {
                                CallStack.Pop();
                            }
                            else
                            {
                                if (WaitTimer < Timing.Global.Milliseconds)
                                {
                                    CommandProcessing.ProcessCommand(curStack.Command, Player, this);
                                    commandsExecuted++;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (CallStack.Count == 0)
                            {
                                PlayerHasDied = false;

                                break;
                            }
                        }

                        curStack = CallStack.Peek();
                    }

                    if (commandsExecuted >= Options.Instance.EventWatchdogKillThreshold)
                    {
                        CallStack.Clear(); //Killing this event, we're over it.
                        if (this.Descriptor.MapId == Guid.Empty)
                        {
                            ApplicationContext.Context.Value?.Logger.LogError(Strings.Events.WatchdogKillCommon.ToString(Descriptor.Name));
                            if (Player.Power.IsModerator)
                            {
                                PacketSender.SendChatMsg(
                                    Player, Strings.Events.WatchdogKillCommon.ToString(Descriptor.Name), ChatMessageType.Error, Color.Red
                                );
                            }
                        }
                        else
                        {
                            ApplicationContext.Context.Value?.Logger.LogError(Strings.Events.WatchdogKill.ToString(map.Name, Descriptor.Name));
                            if (Player.Power.IsModerator)
                            {
                                PacketSender.SendChatMsg(
                                    Player, Strings.Events.WatchdogKill.ToString(map.Name, Descriptor.Name),
                                    ChatMessageType.Error, Color.Red
                                );
                            }
                        }
                    }
                }
                else
                {
                    if (PageInstance.Trigger == EventTrigger.Autorun && WaitTimer < Timing.Global.Milliseconds)
                    {
                        var newStack = new CommandInstance(PageInstance.MyPage);
                        CallStack.Push(newStack);
                    }
                }
            }
        }

        if (PageInstance == null)
        {
            //Try to Spawn a PageInstance.. if we can
            for (var i = Descriptor.Pages.Count - 1; i >= 0; i--)
            {
                if (Conditions.CanSpawnPage(Descriptor.Pages[i], Player, this))
                {
                    if (Global)
                    {
                        if (MapController.TryGetInstanceFromMap(map.Id, Player.MapInstanceId, out var mapInstance))
                        {
                            var globalEvent = mapInstance.GetGlobalEventInstance(Descriptor);
                            if (globalEvent != null)
                            {
                                PageInstance = new EventPageInstance(
                                    Descriptor, Descriptor.Pages[i], Descriptor.Id, MapId, Player.MapInstanceId, this, Player,
                                    globalEvent.GlobalPageInstance[i]
                                );

                                if (PageInstance.GlobalClone != null)
                                {
                                    Player.GlobalPageInstanceLookup.AddOrUpdate(globalEvent.GlobalPageInstance[i], this, (key, oldValue) => this);
                                }

                                sendLeave = false;
                                PageIndex = i;
                            }
                        }
                    }
                    else
                    {
                        PageInstance = new EventPageInstance(Descriptor, Descriptor.Pages[i], MapId, Player.MapInstanceId, this, Player);
                        sendLeave = false;
                        PageIndex = i;
                    }

                    break;
                }
            }

            if (sendLeave && originalPageInstance != null)
            {
                PacketSender.SendEntityLeaveTo(Player, originalPageInstance);
            }
        }
    }

    public Dictionary<string, string> GetParams(Player player)
    {
        var prams = new Dictionary<string, string>();

        foreach (var prm in mParams)
        {
            prams.Add(prm.Key, prm.Value);
        }

        prams.Add("evtName", Descriptor.Name);

        var map = MapController.Get(Descriptor.MapId);
        if (map != null)
        {
            prams.Add("evtMap", map.Name);
        }

        if (MapId != Guid.Empty)
        {
            if (Global && MapController.TryGetInstanceFromMap(MapId, MapInstanceId, out var mapInstance))
            {
                var globalEvent = mapInstance.GetGlobalEventInstance(Descriptor);
                if (globalEvent.GlobalPageInstance != null)
                {
                    prams.Add("evtX", globalEvent.GlobalPageInstance[globalEvent.PageIndex].X.ToString());
                    prams.Add("evtY", globalEvent.GlobalPageInstance[globalEvent.PageIndex].Y.ToString());
                }
            }
            else if (PageInstance != null)
            {
                prams.Add("evtX", PageInstance.X.ToString());
                prams.Add("evtY", PageInstance.Y.ToString());
            }
        }

        if (player != null)
        {
            //Player Name, Map, X, Y, Z?
            //Player Vitals, Player Stats, Player Sprite?
            //More later.. good start now
            prams.Add("plyrName", player.Name);
            prams.Add("plyrMap", player.Map.Name);
            prams.Add("plyrX", player.X.ToString());
            prams.Add("plyrY", player.Y.ToString());
            prams.Add("plyrZ", player.Z.ToString());
            prams.Add("plyrSprite", player.Sprite);
            prams.Add("plyrFace", player.Face);
            prams.Add("plyrLvl", player.Level.ToString());

            //Vitals
            for (var i = 0; i < player.GetVitals().Length; i++)
            {
                prams.Add("plyrVit" + i, player.GetVital(i).ToString());
                prams.Add("plyrMaxVit" + i, player.GetMaxVital(i).ToString());
            }

            //Stats
            var stats = player.GetStatValues();
            for (var i = 0; i < stats.Length; i++)
            {
                prams.Add("plyrStat" + i, stats[i].ToString());
            }
        }

        return prams;
    }

    public void SetParam(string key, string value)
    {
        key = key.ToLower();
        if (mParams.ContainsKey(key))
        {
            mParams[key] = value;
        }
        else
        {
            mParams.Add(key, value);
        }
    }

    public string GetParam(Player player, string key)
    {
        key = key.ToLower();

        var prams = GetParams(player);

        foreach (var pair in prams)
        {
            if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        return "";
    }

    public string FormatParameters(Player player)
    {
        var prams = GetParams(player);
        var output = "{" + Environment.NewLine;
        foreach (var p in prams)
        {
            output += "\t\t\t\"" + p.Key + "\":\t\t\"" + p.Value + "\"," + Environment.NewLine;
        }

        output += "}";

        return output;
    }

}
