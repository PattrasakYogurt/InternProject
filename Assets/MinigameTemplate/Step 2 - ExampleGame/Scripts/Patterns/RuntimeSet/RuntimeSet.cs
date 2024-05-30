namespace MinigameTemplate.Example
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    public static class RuntimeSet
    {
        public enum Group
        {
            Any,

            Player,

            Boss,
            Enemy,
            Npcs,
            Mob,

            Interactable,
            Damageable,
            Pickups,
            Consumables,
            Hazard,

            Checkpoint,
            PlayerSpawnpoint,
            EnemySpawnpoint,
            Target,

            Hint,
            Quest,
            Event,

            Camera,

            Team,
            
            PatrolArea,

            Addressables,
            Swappables,

            WorldCanvas,

            Other,
        }

        public enum Channel
        {
            A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,
        }

        [Serializable]
        public struct Path
        {
            public Group group;
            public Channel channel;

            public Path(Group group, Channel channel)
            {
                this.group = group;
                this.channel = channel;
            }
        }

        public class Set
        {
            public readonly LinkedList<Transform> list;
            public readonly UnityEvent<Transform> onAdd;
            //public readonly UnityEvent<Transform> onRemove;

            public Set()
            {
                this.list = new();
                this.onAdd = new();
                //this.onRemove = new();
            }

            public void Add(Transform target, bool preventDuplicate)
            {
                if (preventDuplicate)
                {
                    LinkedListNode<Transform> node = list.First;

                    while (node != null)
                    {
                        if (target == node.Value) return;
                        node = node.Next;
                    }
                }

                list.AddLast(target);
                onAdd.Invoke(target);
            }

            public bool Remove(Transform target)
            {
                bool removedValue = list.Remove(target);

                //if (removedValue) onRemove.Invoke(target);

                return removedValue;
            }

        }

        private static readonly Dictionary2D<Group, Channel, Set> sets = new();
        private static readonly Dictionary<Transform, HashSet<Path>> instances = new();

        private static readonly LinkedList<Transform> defaultReturn = new();


        public static void AddListenerToOnAddEvent(Group group, Channel channel, UnityAction<Transform> action)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false)
            {
                set = new Set();
                sets.Add(group, channel, set);
            }

            set.onAdd.AddListener(action);
        }
        public static void RemoveListenerToOnRemoveEvent(Group group, Channel channel, UnityAction<Transform> action)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false)
            {
                return;
            }

            set.onAdd.RemoveListener(action);
        }


        public static void Add(Group group, Channel channel, Transform target, bool preventDuplicate = true) 
        {
            if(sets.TryGetValue(group, channel, out Set set) == false)
            {
                set = new Set();
                sets.Add(group, channel, set);
            }

            set.Add(target, preventDuplicate);

            if(instances.TryGetValue(target, out HashSet<Path> paths) == false)
            {
                paths = new();
                instances.Add(target, paths);
            }

            var path = new Path(group, channel);

            paths.Add(path);
        }
        public static bool Remove(Group group, Channel channel, Transform target)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false)
            {
                return false;
            }

            instances.Remove(target);

            if (instances.TryGetValue(target, out HashSet<Path> paths))
            {
                var path = new Path(group, channel);
                paths.Remove(path);
                if(paths.Count == 0) instances.Remove(target);
            }

            return set.Remove(target);
        }

        public static bool Contain(Transform target)
        {
            return instances.ContainsKey(target);
        }

        public static bool ContainPath(Transform target, Path path)
        {
            if (instances.TryGetValue(target, out HashSet<Path> paths) == false) return false;

            return paths.Has(path);
        }
        public static bool ContainAllPaths(Transform target, params Path[] expectedPaths)
        {
            if (instances.TryGetValue(target, out HashSet<Path> paths) == false) return false;

            foreach (var path in expectedPaths)
            {
                if (paths.Has(path) == false) return false;
            }

            return true;
        }
        public static bool ContainSomePaths(Transform target, params Path[] expectedPaths)
        {
            if (instances.TryGetValue(target, out HashSet<Path> paths) == false) return false;

            foreach (var path in expectedPaths)
            {
                if (paths.Has(path)) return true;
            }

            return false;
        }

        public static bool TryGetPaths(Transform target, out HashSet<Path> paths)
        {
            return instances.TryGetValue(target, out paths);
        }
        public static bool TryGetPathsByGroup(Transform target, Group group, out LinkedList<Path> paths)
        {
            paths = new();

            if (instances.TryGetValue(target, out var paths_) == false) return false;

            foreach (var path_ in paths_)
            {
                if(path_.group == group) paths.AddLast(path_);
            }

            return paths.Count > 0;
        }

        #region Get
        public static LinkedList<Transform> Get(Group group, Channel channel = Channel.A)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false) return defaultReturn;


            return set.list;
        }
        public static Transform Get(Group group, Channel channel, int index)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false) return null;


            return set.list.ElementAt(index);
        }

        public static Transform GetRandom(Group group, Channel channel, Predicate<Transform> match = null, System.Random rand = null)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false) return null;

            return set.list.GetRandom(match, rand);
        }
        public static bool Has(Group group, Channel channel, Transform instance)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false) return false;


            return set.list.Has(instance);
        }
        #endregion

        #region Find
        /// <summary>
        /// Find first valid element
        /// </summary>
        /// <param name="filter">
        /// Condition to validate an element
        /// </param>
        /// <returns>
        /// Return a valid element
        /// </returns>
        public static Transform FindFirst(Group group, Channel channel, Predicate<Transform> filter = null)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false) return null;

            if (filter == null) return set.list.First.Value;

            LinkedListNode<Transform> runner = set.list.First;

            while (runner != null)
            {
                if (filter.Invoke(runner.Value)) return runner.Value;
                runner = runner.Next;
            }

            return null;
        }

        /// <summary>
        /// Generate list of valid elements
        /// </summary>
        /// <param name="filter">
        /// Condition to validate an element
        /// </param>
        /// <returns>
        /// Return a list of elements that meet the condition
        /// </returns>
        public static LinkedList<Transform> Find(Group group, Channel channel, Predicate<Transform> filter)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false) return null;


            LinkedListNode<Transform> runner = set.list.First;

            LinkedList<Transform> found = new();

            while (runner != null)
            {
                if (filter.Invoke(runner.Value)) found.AddLast(runner.Value);
                runner = runner.Next;
            }

            return found;
        }
        #endregion

        #region Find Closest
        public static Transform FindClosest(Group group, Channel channel, Vector3 position, float radius = float.PositiveInfinity, Predicate<Transform> filter = null)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false) return null;

            return set.list.FindClosest(position, radius, filter);
        }
        public static LinkedList<Transform> FindAllClosest(Group group, Channel channel, Vector3 position, float radius = float.PositiveInfinity, Predicate<Transform> filter = null)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false) return null;

            return set.list.FindAllClosestTransform(position, radius, filter);
        }

        public static Transform FindClosest(Group group, Channel channel, Transform source, float radius = float.PositiveInfinity, float fovAngle = 360, Predicate<Transform> filter = null)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false) return null;

            return set.list.FindClosestTransform(source, radius, fovAngle, filter);
        }
        public static LinkedList<Transform> FindAllClosest(Group group, Channel channel, Transform source, float radius = float.PositiveInfinity, float fovAngle = 360, Predicate<Transform> filter = null)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false) return null;

            return set.list.FindAllClosestTransform(source, radius, fovAngle, filter);
        }
        #endregion

        #region Other Find Methods
        /// <summary>
        /// Find best element based on its qualifications.
        /// </summary>
        /// <param name="match">
        /// Compare first element with second and return best option.
        /// </param>
        /// <returns>
        /// Best possible element.
        /// </returns>
        public static Transform FindBest(Group group, Channel channel, Func<Transform, Transform, Transform> match)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false) return null;


            Transform target = set.list.First.Value;

            LinkedListNode<Transform> runner = set.list.First;

            while (runner != null)
            {
                target = match.Invoke(target, runner.Value);
                runner = runner.Next;
            }

            return target;
        }
        public static Transform FindRandom(Group group, Channel channel, Predicate<Transform> match, System.Random rand = null)
        {
            LinkedList<Transform> _found = Find(group, channel, match);
            return _found.GetRandom(null, rand);
        }
        public static int Count(Group group, Predicate<Transform> match, Channel channel = Channel.A)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false) return -1;

            LinkedListNode<Transform> runner = set.list.First;

            int count = 0;
            while (runner != null)
            {
                if (match.Invoke(runner.Value)) ++count;
                runner = runner.Next;
            }

            return set.list.Count(match);
        }
        #endregion

        #region Physics
        public static LinkedList<Transform> Find(Group group, Channel channel, LayerMask mask, float _radius, bool onCast = true, bool is2d = false)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false) return null;


            LinkedListNode<Transform> runner = set.list.First;

            LinkedList<Transform> found = new();

            if (is2d == false)
            {
                //Collider[] _collisions = new Collider[1];
                while (runner != null)
                {
                    if (Physics.CheckSphere(runner.Value.transform.position, _radius, mask) == onCast)
                        //if (Physics.OverlapSphereNonAlloc(runner.Value.transform.position, _radius, _collisions, mask) > 0 == onCast)
                        found.AddLast(runner.Value);

                    runner = runner.Next;
                }
            }
            else
            {
                while (runner != null)
                {
                    Collider2D collision = Physics2D.OverlapCircle(runner.Value.transform.position, _radius, mask);

                    if (collision == onCast)
                        found.AddLast(runner.Value);

                    runner = runner.Next;
                }
            }

            return found;
        }
        public static Transform FindFirst(Group group, Channel channel, LayerMask mask, float _radius, bool onCast = true, bool is2d = false)
        {
            if (sets.TryGetValue(group, channel, out Set set) == false) return null;


            LinkedListNode<Transform> runner = set.list.First;

            if (is2d == false)
            {
                //Collider[] _collisions = new Collider[1];
                while (runner != null)
                {
                    if (Physics.CheckSphere(runner.Value.transform.position, _radius, mask) == onCast)
                        //if (Physics.OverlapSphereNonAlloc(runner.Value.transform.position, _radius, _collisions, mask) > 0 == onCast)
                        return runner.Value;

                    runner = runner.Next;
                }
            }
            else
            {
                while (runner != null)
                {
                    Collider2D collision = Physics2D.OverlapCircle(runner.Value.transform.position, _radius, mask);

                    if (collision == onCast)
                        return runner.Value;

                    runner = runner.Next;
                }
            }

            return null;
        }

        internal static void AddListenerToOnAddEvent(object onAddTargetHandler)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}