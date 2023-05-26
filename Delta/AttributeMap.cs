namespace DotNetDelta {

    /// <summary>
    /// A map of attributes. Defines static methods to manipulate attribute maps.
    /// </summary>
    public class AttributeMap : Dictionary<string, object> 
    {
        private static IEqualityComparer<object> comparer = EqualityComparer<object>.Default;


        /// <summary>
        /// Composes two attribute maps into a new attribute map. Composition is just merging the two maps.
        /// </summary>
        public static AttributeMap Compose(AttributeMap a, AttributeMap b, bool keepNull = false) 
        {
            if (a == null)
            {
                a = new AttributeMap();
            }
            if (b == null)
            {
                b = new AttributeMap();
            }
            AttributeMap c = new AttributeMap();
            foreach (KeyValuePair<string, object> kvp in b) 
            {
                if (keepNull || kvp.Value != null)
                {
                    c[kvp.Key] = kvp.Value;
                }
            }
            foreach (KeyValuePair<string, object> kvp in a) 
            {
                if (!b.ContainsKey(kvp.Key)) 
                {
                    c[kvp.Key] = kvp.Value;
                }
                
            }

            return c;
        }


        /// <summary>
        /// Returns the difference between two attribute maps. The difference is an AttributeMap with entries that are in b but not in a.
        /// </summary>
        public static AttributeMap Diff(AttributeMap a, AttributeMap b)
        {
            if (a == null)
            {
                a = new AttributeMap();
            }
            if (b == null)
            {
                b = new AttributeMap();
            }
            AttributeMap diff = new AttributeMap();
            // Get the list of keys in a and b and put them into an array or list
            // Then iterate through the list and compare the values
            // If the values are different, add the key and value to the new map
            // If the values are the same, do nothing
            
            IEnumerable<string> allKeys = a.Keys.Union(b.Keys);

            foreach (string key in allKeys)
            {
                // Case 1, both maps have the key. If their values are equal, they cancel each other out; otherwise, we pick the attribute value in b
                if (a.ContainsKey(key) && b.ContainsKey(key))
                {
                    
                    if (!comparer.Equals(a[key], b[key]))
                    {
                        diff[key] = b[key];
                    }
                }
                // Case 2, key is present in b, but not in a. Add it to the diff map.
                else if (!a.ContainsKey(key) && b.ContainsKey(key))
                {
                    diff[key] = b[key];
                }
                // Case 3, key was present a, but not in b. Add it to the diff map with a null value.
                else if (a.ContainsKey(key) && !b.ContainsKey(key))
                {
                    diff[key] = null;
                }
            }

            return diff;
        }

        /// <summary>
        /// Returns another AttributeMap that would "undo" the changes that the composition of "attributes" and "base" would make to "base".
        /// That is: invert(compose(base, attributes), attributes) == base
        /// </summary>
        public static AttributeMap Invert(AttributeMap modifingAttributes, AttributeMap baseAttributes)
        {
            if (modifingAttributes == null)
            {
                modifingAttributes = new AttributeMap();
            }
            if (baseAttributes == null)
            {
                baseAttributes = new AttributeMap();
            }
            AttributeMap inversion = new AttributeMap();
        
            
            IEnumerable<string> allKeys = modifingAttributes.Keys.Union(baseAttributes.Keys);

            // If key is defined in both maps, take the value in base if they're different; otherwise, don't add anything
            // If key is defined in modifingAttributes, but not in base, use null
            // If key is defined in base, but not in modifingAttributes, don't add anything
            // If key is defined in neither, don't add anything
            foreach (string key in allKeys)
            {
                if (modifingAttributes.ContainsKey(key) && baseAttributes.ContainsKey(key))
                {
                    if (!comparer.Equals(modifingAttributes[key], baseAttributes[key]))
                    {
                        inversion[key] = baseAttributes[key];
                    }
                }
                else if (modifingAttributes.ContainsKey(key) && !baseAttributes.ContainsKey(key))
                {
                    inversion[key] = null;
                }
            }
            return inversion;
        }

        /// <summary>
        /// Returns a new AttributeMap that is the transformation of left when right is superposed. 
        /// Priority determines whether the transformation only applies to attributes that are not defined in left. If true, 
        /// only new attributes are added to the resulting transformation; otherwise, the transformation is just right.
        /// </summary>
        public static AttributeMap? Transform(AttributeMap left, AttributeMap right, bool priority = false)
        {
            if (left == null)
            {
                return right == null ? new AttributeMap() : right;
            }
            if (right == null)
            {
                return new AttributeMap();
            }
            if (!priority)
            {
                // Right argument just overwrites the left argument in case both are defined
                return right;
            }

            // IEnumerable<string> allKeys = left.Keys.Union(right.Keys);
            AttributeMap transformation = new AttributeMap();
            // foreach (string key in allKeys)
            // {
            //     if (!left.ContainsKey(key))
            //     {
            //         transformation[key] = right[key];
            //     }
            // }
            foreach (KeyValuePair<string, object> kvp in right) {
                if (!left.ContainsKey(kvp.Key)) {
                    transformation[kvp.Key] = kvp.Value;
                }
            }
            return transformation;
        }
    }

}
