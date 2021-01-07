using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtendedItemSpawn
{
    [Serializable]
    public class TileNode
    {
        public TileController value;
        public List<TileController> links = new List<TileController>();
        
        [NonSerialized]
        public Dictionary<NodeDirection, TileNode> nodeLinks = new Dictionary<NodeDirection, TileNode>(); 
        [NonSerialized]
        public Dictionary<NodeDirection, TileNode> diagonalLinks = new Dictionary<NodeDirection, TileNode>();

        public int TotalLinks => nodeLinks.Count(l => l.Value.value.corridorTile) 
                                 + diagonalLinks.Count(l => l.Value.value.corridorTile);
    }
}