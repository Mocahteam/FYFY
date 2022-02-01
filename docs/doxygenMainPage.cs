
/*! \mainpage FYFY entry points
 *
 * FYFY is an implementation of Entity Component System (ECS) software architecture for <a href="https://unity.com/">Unity</a>. It is composed of two main parts: the core and several pluggins.
 * 
 * \section core_sec Core module of FYFY
 *
 * With FYFY you develop your game with <see cref="FYFY.FSystem">systems</see>. Systems get <c>GameObjects</c> through <see cref="FYFY.Family">Families</see>. A <see cref="FYFY.Family">Family</see> enables to filter <c>GameObjects</c> thanks to <see cref="FYFY.Matcher">matchers</see>. The <see cref="FYFY.GameObjectManager">GameObjectManager</see> enable to manage your <c>GameObjects</c> (add/remove components, change state...) and keep <see cref="FYFY.Family">Families</see> synchronized.
 * 
 * The mains entry points to FYFY are:
 * <list>
 * 		<item><see cref="FYFY.FSystem"/> the base class to create your <see cref="FYFY.FSystem">systems</see></item>
 * 		<item><see cref="FYFY.Family"/> the mechanism to access similar <c>GameObjects</c> in your <see cref="FYFY.FSystem">systems</see></item>
 * 		<item><see cref="FYFY.FamilyManager"/> the tool te create your <see cref="FYFY.Family">families</see></item>
 * 		<item><see cref="FYFY.Matcher"/> a set of constraint to create your <see cref="FYFY.Family">families</see> with the <see cref="FYFY.FamilyManager">FamilyManager</see></item>
 * 		<item><see cref="FYFY.GameObjectManager"/> the tool to manage your <c>GameObjects</c></item>
 * </list>
 *
 * \section pluggin_sec Pluggins
 *
 * <list>
 * 		<item>The PointerManager: <see cref="FYFY_plugins.PointerManager.PointerSensitive" /></item>
 * 		<item>The TriggerManager: for 2D <see cref="FYFY_plugins.TriggerManager.TriggerSensitive2D" /> and 3D <see cref="FYFY_plugins.TriggerManager.TriggerSensitive3D" /></item>
 * 		<item>The CollisionManager: for 2D <see cref="FYFY_plugins.CollisionManager.CollisionSensitive2D" /> and 3D <see cref="FYFY_plugins.CollisionManager.CollisionSensitive3D" /></item>
 * 		<item>The MonitoringManager: <see cref="FYFY_plugins.Monitoring.MonitoringManager" /></item>
 * </list>
 *
 */