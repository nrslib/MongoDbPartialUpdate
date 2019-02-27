using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace MongoDbPartialUpdateModel.UpdateBuilder
{
    public class DictionaryUpdateDefinitionBuilder
    {
        public UpdateDefinition<T> BuildCominedUpdateDefine<T>(string rootName, IDictionary original, IDictionary after)
        {
            var updates = BuildUpdateDefine<T>(rootName, original, after);
            return Builders<T>.Update.Combine(updates);
        }

        /// <summary>
        /// Create query for $set and %unset.
        /// Remove node, remove leaf, insert leaf, update leaf
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootName"></param>
        /// <param name="original"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        public List<UpdateDefinition<T>> BuildUpdateDefine<T>(string rootName, IDictionary original, IDictionary after)
        {
            var originals = GetLeafs(rootName, original);
            var afterEmptyNodes = new HashSet<string>();
            var afters = GetLeafs(rootName, after, emptyKey => afterEmptyNodes.Add(emptyKey));

            var removeNodeUpdates = GenerateUnsetDefines<T>(afterEmptyNodes);

            var removedValues = originals
                .Where(x => !afters.TryGetValue(x.Key, out _))
                .Where(x => !afterEmptyNodes.Any(removeKey => x.Key.StartsWith(removeKey))) // the node will be removed.
                .Select(x => x.Key);
            var removeLeafUpdates = GenerateUnsetDefines<T>(removedValues);

            var insertedValues = afters
                .Where(x => !originals.TryGetValue(x.Key, out _));
            var insertLeafUpdates = GenerateSetUpdateDefines<T>(insertedValues);

            var updatedValues = afters
                .Where(x => originals.TryGetValue(x.Key, out var orgValue) && !x.Value.Equals(orgValue));
            var updateLeafUpdates = GenerateSetUpdateDefines<T>(updatedValues);

            return removeNodeUpdates
                .Concat(removeLeafUpdates)
                .Concat(insertLeafUpdates)
                .Concat(updateLeafUpdates)
                .ToList();
        }

        private Dictionary<string, object> GetLeafs(string rootPath, IDictionary data, Action<string> emptyFound = null)
        {
            return GetLeafs(rootPath, data, new Dictionary<string, object>(), emptyFound);
        }

        private Dictionary<string, object> GetLeafs(string path, IDictionary data, Dictionary<string, object> acc, Action<string> emptyFound = null)
        {
            if (data.Count == 0)
            {
                emptyFound?.Invoke(path);
                return acc;
            }

            foreach (DictionaryEntry o in data)
            {
                var key = o.Key.ToString();
                var completePath = path + "." + key;
                switch (o.Value)
                {
                    case IDictionary dictionary:
                        GetLeafs(completePath, dictionary, acc, emptyFound);
                        break;
                    default:
                        acc.Add(completePath, o.Value);
                        break;
                }
            }

            return acc;
        }

        private IEnumerable<UpdateDefinition<T>> GenerateUnsetDefines<T>(IEnumerable<string> data)
        {
            var list = data.ToList();
            if (!list.Any())
            {
                return Enumerable.Empty<UpdateDefinition<T>>();
            }

            return list.Select(x => Builders<T>.Update.Unset(x));
        }

        private IEnumerable<UpdateDefinition<T>> GenerateSetUpdateDefines<T>(IEnumerable<KeyValuePair<string, object>> data)
        {
            var map = data.ToDictionary(x => x.Key, x => x.Value);
            if (!map.Any())
            {
                return Enumerable.Empty<UpdateDefinition<T>>();
            }

            return map.Select(x => Builders<T>.Update.Set(x.Key, x.Value));
        }
    }
}
