using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ExtendedItemSpawn
{
    public enum NodeDirection
    {
        Top, Bottom, Left, Right, TopLeft, TopRight, BottomLeft, BottomRight
    }
    
    [Serializable]
    public class LevelTree
    {
        public List<TileNode> nodes = new List<TileNode>();

        public List<SubRoom> subRooms = new List<SubRoom>();
        
        public List<TileNode> deadEnds = new List<TileNode>();
        
        public void InitializeTree(List<TileController> tiles, List<RoomController> rooms, float tileSize)
        {
            var checkPositions = new Dictionary<NodeDirection, Vector3>
            {
                {NodeDirection.Top, new Vector3(0, 0, tileSize)}, //Top
                {NodeDirection.Bottom, new Vector3(0, 0, -tileSize)}, //Bottom
                {NodeDirection.Right, new Vector3(tileSize, 0, 0)}, //Right
                {NodeDirection.Left, new Vector3(-tileSize, 0, 0)}, //Left
            };

            var cornerPositions = new Dictionary<NodeDirection, Vector3>
            {
                {NodeDirection.TopLeft, new Vector3(-tileSize, 0, tileSize)}, //Top left
                {NodeDirection.BottomLeft, new Vector3(-tileSize, 0, -tileSize)}, //Bottom left
                {NodeDirection.TopRight, new Vector3(tileSize, 0, tileSize)}, //Top right
                {NodeDirection.BottomRight, new Vector3(tileSize, 0, -tileSize)} //Bottom right
            };

            foreach (var tile in tiles)
            {
                nodes.Add(new TileNode{ value = tile });   
            }
            
            foreach (var tile in tiles)
            {
                var node = nodes.Single(n => n.value == tile);
                
                var thisPosition = tile.transform.position;
                foreach (var checkPosition in checkPositions)
                {
                    var linkedTile = tiles.FirstOrDefault(t => thisPosition + checkPosition.Value == t.transform.position);
                    //var linkedTile = tiles.SingleOrDefault(t => thisPosition + checkPosition.Value == t.transform.position);
                    if (linkedTile != null)
                    {
                        node.links.Add(linkedTile);
                        var linkedNode = nodes.Single(n => n.value == linkedTile);
                        node.nodeLinks.Add(checkPosition.Key, linkedNode);
                    }
                }

                if (node.nodeLinks.ContainsKey(NodeDirection.Top) && node.nodeLinks.ContainsKey(NodeDirection.Right))
                    AddDiagonalIfExist(tiles, cornerPositions, thisPosition, node, NodeDirection.TopRight);
                if (node.nodeLinks.ContainsKey(NodeDirection.Top) && node.nodeLinks.ContainsKey(NodeDirection.Left))
                    AddDiagonalIfExist(tiles, cornerPositions, thisPosition, node, NodeDirection.TopLeft);
                if (node.nodeLinks.ContainsKey(NodeDirection.Bottom) && node.nodeLinks.ContainsKey(NodeDirection.Left))
                    AddDiagonalIfExist(tiles, cornerPositions, thisPosition, node, NodeDirection.BottomLeft);
                if (node.nodeLinks.ContainsKey(NodeDirection.Bottom) && node.nodeLinks.ContainsKey(NodeDirection.Right))
                    AddDiagonalIfExist(tiles, cornerPositions, thisPosition, node, NodeDirection.BottomRight);
            }

            FindSubRooms();
            FindDeadEnds();
        }

        private void FindDeadEnds()
        {
            foreach (var node in nodes.Where(n => n.value.corridorTile))
            {
                //Has single connection and not a SubRoom node.
                if (node.TotalLinks == 1 && node.links.All(l => l.corridorTile) && !node.links
                    .Any(n => subRooms
                        .Any(s => s.nodes.Any(sn => sn.value == n))))
                {
                    deadEnds.Add(node);
                }
            }
        }

        private void AddDiagonalIfExist(List<TileController> tiles, Dictionary<NodeDirection, Vector3> cornerPositions, 
            Vector3 thisPosition, TileNode node, NodeDirection diagonalDirection)
        {
            var checkPosition = cornerPositions[diagonalDirection];
//            var diagonalTile = tiles.SingleOrDefault(t => thisPosition + checkPosition == t.transform.position);
            var diagonalTile = tiles.FirstOrDefault(t => thisPosition + checkPosition == t.transform.position);
            if (diagonalTile)
            {
                var diagonalNode = nodes.Single(n => n.value == diagonalTile);
                node.diagonalLinks.Add(diagonalDirection, diagonalNode);
            }
        }
        
        private void FindSubRooms()
        {
            var alreadyInSubRoom = new List<TileNode>();
            
            foreach (var node in nodes.Where(n => n.value.corridorTile))
            {
                var subRoom = new SubRoom();
                SubRoomRecursiveSearch(subRoom, alreadyInSubRoom, node);
                if (subRoom.nodes.Count > 1) //Exclude single crossroad node with 3 ways
                    subRooms.Add(subRoom);
            }
        }

        private void SubRoomRecursiveSearch(SubRoom subRoom, List<TileNode> alreadyInSubRoom, TileNode node)
        {
            if(alreadyInSubRoom.Contains(node) || node.TotalLinks == 2 || node.links.Any(n => !n.corridorTile))
                return;

            subRoom.nodes.Add(node);
            alreadyInSubRoom.Add(node);

            foreach (var child in node.nodeLinks.Where(n => n.Value.value.corridorTile))
                SubRoomRecursiveSearch(subRoom, alreadyInSubRoom, child.Value);
        }
    }
}
