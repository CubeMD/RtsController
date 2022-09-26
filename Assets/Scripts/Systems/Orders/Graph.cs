using System.Collections.Generic;

namespace Systems.Orders
{
    public class Graph
    {
        private List<Node> nodes;
        private List<Edge> edges;

        public Node NewNode(INode node)
        {
            Node newNode = new Node();
            return node.UpdateNode(newNode);;
        }

        public Edge NewEdge(Node from, Node to)
        {
            return new Edge();
        }
    }

    public interface INode
    {
        public Node UpdateNode(Node node);
    }
    
    public class Node
    {
        public float[] data;
        
        public Node()
        {
            
        }
    }

    public class Edge
    {
        public float[] data;
        
        public Edge()
        {
            
        }
    }
}