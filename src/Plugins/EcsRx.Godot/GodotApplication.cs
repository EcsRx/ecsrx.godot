using System.Collections.Generic;
using System.Linq;
using EcsRx.Collections;
using EcsRx.Collections.Database;
using EcsRx.Infrastructure;
using EcsRx.Infrastructure.Modules;
using EcsRx.Infrastructure.Ninject;
using EcsRx.Plugins.ReactiveSystems;
using EcsRx.Plugins.Views;
using Godot;
using SystemsRx.Events;
using SystemsRx.Executor;
using SystemsRx.Extensions;
using SystemsRx.Infrastructure.Dependencies;
using SystemsRx.Infrastructure.Extensions;
using SystemsRx.Infrastructure.Modules;
using SystemsRx.Infrastructure.Plugins;
using SystemsRx.Systems;

namespace EcsRx.Godot
{
    public abstract class GodotApplication : Node, IEcsRxApplication
    {
        public IDependencyContainer Container { get; } = new NinjectDependencyContainer();

        public ISystemExecutor SystemExecutor { get; private set; }
        public IEventSystem EventSystem { get; private set; }
        public IEntityDatabase EntityDatabase { get; private set; }
        public IObservableGroupManager ObservableGroupManager { get; private set; }
        public IEnumerable<ISystemsRxPlugin> Plugins => _plugins;
        
        private List<ISystemsRxPlugin> _plugins { get; } = new List<ISystemsRxPlugin>();

        protected abstract void ApplicationStarted();

        public override void _EnterTree()
        {
            StartApplication();
        }

        public override void _ExitTree()
        {
            StopApplication();
        }

        public virtual void StartApplication()
        {
            LoadModules();
            LoadPlugins();
            SetupPlugins();
            ResolveApplicationDependencies();
            BindSystems();
            StartPluginSystems();
            StartSystems();
            ApplicationStarted();
        }
        
        public virtual void StopApplication()
        { StopAndUnbindAllSystems(); }

        /// <summary>
        /// Load any plugins that your application needs
        /// </summary>
        /// <remarks>It is recommended you just call RegisterPlugin method in here for each plugin you need</remarks>
        protected virtual void LoadPlugins()
        {
            RegisterPlugin(new ViewsPlugin());
            RegisterPlugin(new ReactiveSystemsPlugin());
        }

        /// <summary>
        /// Load any modules that your application needs
        /// </summary>
        /// <remarks>
        /// If you wish to use the default setup call through to base, if you wish to stop the default framework
        /// modules loading then do not call base and register your own internal framework module.
        /// </remarks>
        protected virtual void LoadModules()
        {
            Container.LoadModule<EcsRxInfrastructureModule>();
            Container.LoadModule<FrameworkModule>();
        }
        
        /// <summary>
        /// Resolve any dependencies the application needs
        /// </summary>
        /// <remarks>By default it will setup SystemExecutor, EventSystem, EntityCollectionManager</remarks>
        protected virtual void ResolveApplicationDependencies()
        {
            EventSystem = Container.Resolve<IEventSystem>();
            SystemExecutor = Container.Resolve<ISystemExecutor>();
            EntityDatabase = Container.Resolve<IEntityDatabase>();
            ObservableGroupManager = Container.Resolve<IObservableGroupManager>();
        }

        /// <summary>
        /// Bind any systems that the application will need
        /// </summary>
        /// <remarks>By default will auto bind any systems within application scope</remarks>
        protected virtual void BindSystems()
        { this.BindAllSystemsWithinApplicationScope(); }
        
        protected virtual void StopAndUnbindAllSystems()
        {
            var allSystems = SystemExecutor.Systems.ToList();
            allSystems.ForEach(SystemExecutor.RemoveSystem);
            Container.Unbind<ISystem>();
        }

        /// <summary>
        /// Start any systems that the application will need
        /// </summary>
        /// <remarks>By default it will auto start any systems which have been bound</remarks>
        protected virtual void StartSystems()
        { this.StartAllBoundSystems(); }
        
        protected virtual void SetupPlugins()
        { _plugins.ForEach(x => x.SetupDependencies(Container)); }

        protected virtual void StartPluginSystems()
        {
            _plugins.SelectMany(x => x.GetSystemsForRegistration(Container))
                .ForEachRun(x => SystemExecutor.AddSystem(x));
        }

        protected void RegisterPlugin(ISystemsRxPlugin plugin)
        { _plugins.Add(plugin); }
    }
}