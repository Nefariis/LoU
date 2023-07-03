using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Component = UnityEngine.Component;

namespace LoU
{
    internal static class Extensions
    {
        

        internal static bool Contains2(this string source, string toCheck)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(toCheck)) return false;
            return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        internal static bool IsNotNullOrEmpty(this string source)
        {
            return !string.IsNullOrEmpty(source);
        }

        internal static bool IsNullOrEmpty<T>(this List<T> source)
        {
            return source.IsNull() || source.Count == 0;
        }
        
        internal static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }

        internal static bool IsNull<T>(this T source)
        {
            return source == null;
        }

        internal static bool IsNotNull<T>(this T source)
        {
            return source != null;
        }

        internal static bool IsEmpty(this string source)
        {
            return source == "";
        }

        internal static bool IsDictionary(this object o)
        {
            if (o.IsNull()) return false;
            return o is IDictionary &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }
        
        internal static float DistanceFrom(this Vector3 a, Vector3 b)
        {
            if (a.IsNull() || b.IsNull()) return 0;
            var num1 = a.x - b.x;
            var num2 = a.y - b.y;
            var num3 = a.z - b.z;
            return (float) Math.Sqrt((double) num1 * num1 + (double) num2 * num2 + (double) num3 * num3);
        }
        
        internal static Vector3 RelativePositionFrom(this Transform transform, Transform ancestor)
        {
            Vector3 position = transform.localPosition;
            Transform t = transform.parent;
            
            while (t.IsNotNull() && t != ancestor)
            {
                position = position + t.localPosition;
                t = t.parent;
            }
            return position;
        }

        internal static void Enumerate<T>(this IEnumerable<T> ie, Action<T, int> action)
        {
            var i = 0;
            foreach (var e in ie) action(e, i++);
        }
    }

    internal static class Utils
    {

        public static Dictionary<string, FloatingPanel> FindPanelByName(string name)
        {

            var foundPanels = new Dictionary<string, FloatingPanel>();
            var fpm = FloatingPanelManager.DJCGIMIDOPB;
            if (!fpm) return foundPanels;

            var panels = (List<FloatingPanel>) InstanceFields.GetInstanceField(fpm, "AGLMPFPPEDK");
            if (panels.IsNullOrEmpty()) return foundPanels;

            foreach (var floatingPanel in panels)
            {
                if (name.IsNullOrEmpty() || floatingPanel.PanelId.Contains2(name))
                    foundPanels.Add(floatingPanel.PanelId, floatingPanel);
            }

            Logging.Log($"[FindPanelByName] - Found {foundPanels.Count} panels by {name}.");
            return foundPanels;
        }

        public static Dictionary<string, DynamicObject> FindDynamicObjectsByName(string name)
        {

            var objects = Enumerable.ToList(ClientObjectManager.DJCGIMIDOPB.MFGFGOCNCDG);
            Dictionary<string, DynamicObject> foundObjects = new Dictionary<string, DynamicObject>();

            foreach (var obj in objects)
            {
                if (obj.EBHEDGHBHGI.Contains2(name))
                    foundObjects.Add(obj.ObjectId.ToString(), obj);
            }

            Logging.Log($"[FindDynamicObjectsByName] - Found {foundObjects.Count()} dynamic objects by {name}.");
            return foundObjects;
        }

        public static Dictionary<string, DynamicObject> FindDynamicObjectsByName(string name, ulong containerId)
        {
            var foundObjects = new Dictionary<string, DynamicObject>();
            var objects = ClientObjectManager.DJCGIMIDOPB.GetObjectsInContainer(containerId);

            foreach (DynamicObject obj in objects)
            {
                if (name.IsNullOrEmpty() || obj.EBHEDGHBHGI.Contains2(name))
                    foundObjects.Add(obj.ObjectId.ToString(), obj);
            }

            Logging.Log(
                $"[FindDynamicObjectsByName] - Found {foundObjects.Count} dynamic objects by {name} and {containerId}.");
            return foundObjects;
        }
        
        public static DynamicObject FindDynamicObject(ulong objectId, ulong containerId = 0)
        {
            if (containerId > 0)
            {
                foreach (DynamicObject obj in ClientObjectManager.DJCGIMIDOPB.GetObjectsInContainer(containerId))
                {
                    if (obj.ObjectId == objectId)
                        return obj;
                }
            }
            else
            {
                DynamicObject dynamicObject = ClientObjectManager.DJCGIMIDOPB.GetDynamicObjectById(objectId);
                return dynamicObject;
            }

            Logging.Log($"[FindDynamicObject] - Did not find Dynamic Object by id {objectId} and {containerId}.");
            return null;
        }

        public static ClientObject FindClientObject(ulong objectId, ulong containerId = 0)
        {
            if (containerId > 0)
            {
                foreach (DynamicObject obj in ClientObjectManager.DJCGIMIDOPB.GetObjectsInContainer(containerId))
                {
                    if (obj.ObjectId == objectId)
                        return obj.GetComponent<ClientObject>();
                }
            }
            else
            {
                ClientObject clientObject = ClientObjectManager.DJCGIMIDOPB.GetClientObjectById(objectId);
                if (!clientObject.IsNull())
                    return clientObject;
            }

            Logging.Log($"[FindClientObject] - Did not find Client Object by id {objectId} and {containerId}.");
            return null;
        }

        public static ClientObject FindPermanentObject(int permanentId)
        {
            return ClientObjectManager.DJCGIMIDOPB.GetPermanentObjectById(permanentId);
        }

        public static Dictionary<string, ClientObject> FindPermanentObjectByName(string name, float distance = 50)
        {
            var foundObjects = new Dictionary<string, ClientObject>();

            IEnumerable objects = ClientObjectManager.DJCGIMIDOPB.PermanentObjectLookup.Values.OrderBy(obj =>
                Vector3.Distance(obj.transform.position,
                    GameObjectSingleton<ApplicationController>.DJCGIMIDOPB.Player.transform.position));

            foreach (ClientObject obj in objects)
            {
                if (Vector3.Distance(obj.transform.position,
                    GameObjectSingleton<ApplicationController>.DJCGIMIDOPB.Player.transform.position) > distance)
                    break;

                if (obj.name.IsNotNullOrEmpty() && (name.IsEmpty() || obj.name.Contains2(name)))
                    foundObjects.Add(obj.PermanentId.ToString(), obj);
            }

            Logging.Log(
                $"[FindPermanentObjectByName] - Found {foundObjects.Count} objects by {name} with dist of {distance}.");
            return foundObjects;
        }

        public static IEnumerable<MobileInstance> GetNearbyMobiles(float distance)
        {
            if (ClientObjectManager.DJCGIMIDOPB.IsNotNull())
                return ClientObjectManager.DJCGIMIDOPB.GetNearbyMobiles(distance);
            return ClientObjectManager.DJCGIMIDOPB.GetNearbyMobiles(50);
        }

        public static MobileInstance GetMobile(ulong objectId)
        {
            return ClientObjectManager.DJCGIMIDOPB.GetMobileObjectById(objectId);
        }

        public static List<MobileInstance> FindMobile(string name, float distance = 50)
        {
            var playerPosition = GameObjectSingleton<ApplicationController>.DJCGIMIDOPB.Player.transform.position;
            var foundMobiles = new List<MobileInstance>();
            var nearbyMobiles = GetNearbyMobiles(distance).ToArray();
            if (nearbyMobiles.IsNullOrEmpty()) return foundMobiles;

            var mobiles = nearbyMobiles.OrderBy(obj =>
                obj.transform.position.DistanceFrom(playerPosition)).ToArray();

            if (mobiles.IsNullOrEmpty()) return foundMobiles;

            foreach (MobileInstance mobile in mobiles)
            {
                if (mobile.transform.position.DistanceFrom(playerPosition) > distance)
                {
                    break;
                }

                if (mobile.EBHEDGHBHGI.IsNotNullOrEmpty() &&
                    (name.IsEmpty() || mobile.EBHEDGHBHGI.Contains2(name) && !foundMobiles.Contains(mobile)))
                {
                    foundMobiles.Add(mobile);
                }
            }

            Logging.Log(
                $"[FindMobile] - Found {foundMobiles.Count} objects by {name} with dist of {distance}.");
            return foundMobiles;
        }

    }

    internal static class InstanceFields
    {
        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            if (instance.IsNull()) return null;
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var field = type.GetField(fieldName, bindFlags);
            return field?.GetValue(instance);
        }

        public static object GetInstanceField<T>(T instance, string fieldName)
        {
            if (instance.IsNull()) return null;
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var field = typeof(T).GetField(fieldName, bindFlags);
            return field?.GetValue(instance);
        }

        public static void SetInstanceField<T1>(T1 instance, string fieldName, object value)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var field = typeof(T1).GetField(fieldName, bindFlags);
            field?.SetValue(instance, value);
        }
    }

    internal static class Logging {
        
        private static bool DEBUGGING = false;
        
        private static readonly List<Type> Components = new List<Type> {typeof(DynamicWindow), typeof(UIWidget), 
            typeof(BoxCollider), typeof(DynamicWindowTwoLabelButton), typeof(DynamicWindowDefaultButton), 
            typeof(UIImageButton), typeof(UIPlaySound), typeof(UIButtonMessage), typeof(DynamicWindowScrollableLabel), 
            typeof(UIEventListener), typeof(BoxCollider), typeof(BoxCollider), typeof(UIEventListener), typeof(UILabel)};

        public static void Log(string s)
        {
            if (DEBUGGING) Debug.Log($"LoU - {DateTime.UtcNow:o} - {s}");
        }

        public static void Log(MonoBehaviour c)
        {
            if (!DEBUGGING) return;

            if (c.gameObject && c.gameObject.GetComponent<UIEventListener>())
                Log("c.gameObject has listener!");
            if (c.transform && c.transform.GetComponent<UIEventListener>())
                Log("c.transform has listener!");

            foreach (var component in Components)
            {
                var compValue = c.GetComponent(component);
                if(c.GetComponent(component).IsNull()) continue;

                Log(component.ToString());
                LogProps(compValue);

                if (component != typeof(UILabel)) continue;
                var uiLabel = (UILabel) compValue;

                if (uiLabel.GBHBIODJFCD != "[412A08]Craft") continue;
                Log("CRAFT BUTTON FOUND");
                    
                if (uiLabel.GetComponent<UIEventListener>().IsNull()) continue;
                Log("uiLabel has listener!");
                    
                if (uiLabel.transform.IsNotNull())
                    Log("uiLabel.transform has listener!");
                if (uiLabel.gameObject.IsNotNull())
                    Log("uiLabel.gameObject has listener!");
            }
        }

        private static void Log(GameObject c)
        {
            if (!DEBUGGING) return;
            
            if (c.gameObject && c.gameObject.GetComponent<UIEventListener>())
                Log($"GameObject Name: {c.gameObject.name} Tag: {c.gameObject.tag} Has Listener!");
            if (c.transform && c.transform.GetComponent<UIEventListener>())
                Log($"Transform Name: {c.transform.name} Tag: {c.transform.tag} Has Listener!");
            
            Log($"Enumerating Through Components {c.GetComponents<Component>()}");

            c.GetComponents<Component>().Enumerate((comp, i) =>
            {
                Log($"i: {i.ToString()} | name: {comp.name} | tag: {comp.tag} | type: {comp.GetType()}");
                LogProps(comp);

                if (comp.GetType() != typeof(UILabel)) return;
                var uiLabel = (UILabel) comp;

                if (uiLabel.GBHBIODJFCD != "[412A08]Craft") return;
                Log("CRAFT BUTTON FOUND");

                if (!uiLabel.GetComponent<UIEventListener>()) return;
                Log("uiLabel has listener!");

                if (uiLabel.transform.IsNotNull())
                    Log("uiLabel.transform has listener!");
                if (uiLabel.gameObject.IsNotNull())
                    Log("uiLabel.gameObject has listener!");
            });
            

            foreach (Component component in c.GetComponents<Component>())
            {
                if(!component) continue;

                Log(component.ToString());

            }
        }

        public static void Log(Transform o)
        {
            
            var children = Enumerable.ToList(
                from GameObject child in o.transform select child.gameObject);
            
            Log($"*** CHILDREN LOG {o.name} ({children.Count}) START ***");
            
            foreach (var c in children)
            {
                LogProps(c);
                
                if (c.GetComponent<UIEventListener>())
                    Log("child has listener!");
                if (c.gameObject && c.gameObject.GetComponent<UIEventListener>())
                    Log("child.gameObject has listener!");
                if (c.transform && c.transform.GetComponent<UIEventListener>())
                    Log("child.transform has listener!");


                //GBHBIODJFCD =[412A08] / Craft All
                Log("trying to enumerate " + c.GetComponents<UnityEngine.Component>());
                int i = 0;
                foreach (var comp in c.GetComponents<UnityEngine.Component>())
                {
                    i = i + 1;
                    Log(i.ToString());
                    Log(comp.name);
                    Log(comp.tag);
                    Log(comp.GetType().ToString());
                }
                Log("finish");
                Log(c);
            }
            Log($"*** CHILDREN LOG {o.name} ({children.Count}) END ***");
        }

        public static void LogObject(DynamicObject obj)
        {
            Log( $"--- OBJECT {obj.GetInstanceID()} START ---");
            LogProps(obj);
            Log("***CLIENT OBJECT***");
            LogProps(obj.AOJMJNFMBJO);
            Log("***TRANSFORM***");
            LogProps(obj.transform);
            Log("***GAME OBJECT***");
            LogProps(obj.gameObject);
            Log( $"--- OBJECT {obj.GetInstanceID()} END ---");
        }

        private static void LogProps(object obj)
        {
            Log("--- PROPS START ---");
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                var name = descriptor.Name;
                var value = descriptor.GetValue(obj);
                Log($"{name} = {value}");
                
                if (!value.IsDictionary()) continue;
                var collection = ((IDictionary) value)?.Keys;
                
                if (collection == null) continue;
                foreach (string key in collection)
                {
                    Log($"Key:{name}.{key} = Value:{name}.{((IDictionary) value)[key]}");
                }
            }

            foreach (var f in obj.GetType().GetFields().Where(f => f.IsPrivate | f.IsPublic | f.IsStatic))
            {
                Log($"Field: {f.Name} Value: {f.GetValue(obj)}");
            }
            Log("--- PROPS END ---");
        }
    }
}
