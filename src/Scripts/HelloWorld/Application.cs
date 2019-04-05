using EcsRx.Extensions;
using EcsRx.Godot;
using Scripts.HelloWorld.Components;

namespace Scripts.HelloWorld
{
    public class Application : GodotApplication
    {
        protected override void ApplicationStarted()
        {
            var collection = EntityCollectionManager.GetCollection();
            var entity = collection.CreateEntity();

            var helloComponent = new SayHelloComponent
            {
                Name = "Bob"
            };
            entity.AddComponent(helloComponent);
        }
    }
}
