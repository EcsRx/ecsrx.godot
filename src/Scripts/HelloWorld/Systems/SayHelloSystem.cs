using System;
using System.Reactive.Linq;
using EcsRx.Entities;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Groups.Observable;
using EcsRx.Plugins.ReactiveSystems.Systems;
using Godot;
using Scripts.HelloWorld.Components;

namespace Scripts.HelloWorld.Systems
{
    public class SayHelloSystem : IReactToGroupSystem
    {
        public IGroup Group { get; } = new Group(typeof(SayHelloComponent));
        
        public IObservable<IObservableGroup> ReactToGroup(IObservableGroup observableGroup)
        { return Observable.Interval(TimeSpan.FromSeconds(1)).Select(x => observableGroup); }

        public void Process(IEntity entity)
        {
            var helloComponent = entity.GetComponent<SayHelloComponent>();
            GD.Print($"Hello there {helloComponent.Name} @ {DateTime.Now}");
        }
    }
}