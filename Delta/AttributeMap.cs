namespace DotNetDelta {

    /// <summary>
    /// A map of attributes. Defines static methods to manipulate attribute maps.
    /// </summary>
    class AttributeMap : Dictionary<string, object> {


        /// <summary>
        /// Composes two attribute maps into a new attribute map. Composition is just merging the two maps.
        /// </summary>
        public static AttributeMap compose(AttributeMap a, AttributeMap b, bool keepNull = false) {
            if (a == null)
            {
                a = new AttributeMap();
            }
            if (b == null)
            {
                b = new AttributeMap();
            }
            AttributeMap c = new AttributeMap();
            foreach (KeyValuePair<string, object> kvp in a) {
                if (keepNull || kvp.Value != null) 
                {
                    c[kvp.Key] = kvp.Value;
                }
                
            }
            foreach (KeyValuePair<string, object> kvp in b) {
                if (keepNull || kvp.Value != null)
                {
                    c[kvp.Key] = kvp.Value;
                }
            }
            return c;
        }


        /// <summary>
        /// Returns the difference between two attribute maps (b - a). The difference is an AttributeMap with entries that are in b but not in a.
        /// </summary>
        public static AttributeMap diff(AttributeMap a, AttributeMap b)
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
            
            string[] allKeys = new string[a.Count + b.Count];
            a.Keys.CopyTo(allKeys, 0);
            b.Keys.CopyTo(allKeys, a.Count);

            foreach (string key in allKeys)
            {
                // Case 1, both maps has the key, and their values are the equals. They cancel each other out.
                // Case 2, key is present in a, but not in b. Add it to the diff map.
                if (a.ContainsKey(key) && b.ContainsKey(key))
                {
                    if (!a[key].Equals(b[key]))
                    {
                        diff[key] = b[key];
                    }
                }
                else if (!a.ContainsKey(key) && b.ContainsKey(key))
                {
                    diff[key] = b[key];
                }
            }

            return diff;
        }

        /// <summary>
        /// Returns another AttributeMap that would "undo" the changes that the composition of "attributes" and "base" would make to "base".
        /// That is: invert(compose(base, attributes), attributes) == base
        /// </summary>
        public static AttributeMap invert(AttributeMap modifingAttributes, AttributeMap baseAttributes)
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
        
            
            string[] allKeys = new string[modifingAttributes.Count + baseAttributes.Count];
            modifingAttributes.Keys.CopyTo(allKeys, 0);
            modifingAttributes.Keys.CopyTo(allKeys, modifingAttributes.Count);
            // If key is defined in both maps, take the value in base
            // If key is defined in modifingAttributes, but not in base, use null
            // If key is defined in base, but not in modifingAttributes, don't add anything
            // If key is defined in neither, don't add anything
            foreach (string key in allKeys)
            {
                if (modifingAttributes.ContainsKey(key) && baseAttributes.ContainsKey(key))
                {
                    inversion[key] = baseAttributes[key];
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
        /// </summary>
        public static AttributeMap transform(AttributeMap left, AttributeMap right, bool priority = false)
        {
            if (left == null)
            {
                left = new AttributeMap();
            }
            if (right == null)
            {
                right = new AttributeMap();
            }
            if (!priority)
            {
                return right;
            }
            string[] allKeys = new string[left.Count + right.Count];
            left.Keys.CopyTo(allKeys, 0);
            right.Keys.CopyTo(allKeys, left.Count);

            AttributeMap transformation = new AttributeMap();
            foreach (string key in allKeys)
            {
                if (!left.ContainsKey(key))
                {
                    transformation[key] = right[key];
                }
            }
            return transformation;
        }
    }

}