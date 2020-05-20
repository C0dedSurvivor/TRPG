using System.Collections.Generic;

/*
 * Just ignore this
 * It has no reason to exist
 * It's only still here in case I need it later
 */

public class GraphNode<T>
{
    // Private member-variables
    private T data;
    private NodeList<T> neighbors = null;

    private List<int> costs;

    public GraphNode() { }
    public GraphNode(T value) : this(value, null) { }
    public GraphNode(T value, NodeList<T> neighbors)
    {
        data = value;
        this.neighbors = neighbors;
    }

    public T Value
    {
        get
        {
            return data;
        }
        set
        {
            data = value;
        }
    }

    public NodeList<T> Neighbors
    {
        get
        {
            if (neighbors == null)
                neighbors = new NodeList<T>();

            return neighbors;
        }
    }

    public List<int> Costs
    {
        get
        {
            if (costs == null)
                costs = new List<int>();

            return costs;
        }
    }
}

public class NodeList<T>
{
    public List<GraphNode<T>> list;

    public void Add(T value)
    {
        list.Add(new GraphNode<T>(value));
    }

    public void Add(GraphNode<T> node)
    {
        list.Add(node);
    }

    public void Remove(GraphNode<T> node)
    {
        list.Remove(node);
    }

    public void RemoveAt(int index)
    {
        list.RemoveAt(index);
    }

    public int Count
    {
        get
        {
            return list.Count;
        }
    }

    public int IndexOf(GraphNode<T> node)
    {
        return list.IndexOf(node);
    }

    public GraphNode<T> FindByValue(T value)
    {
        // search the list for the value
        foreach (GraphNode<T> node in list)
            if (node.Value.Equals(value))
                return node;

        // if we reached here, we didn't find a matching node
        return null;
    }
}

public class Graph<T>
{
    private NodeList<T> nodeSet;

    public Graph() : this(null) { }
    public Graph(NodeList<T> nodeSet)
    {
        if (nodeSet == null)
            this.nodeSet = new NodeList<T>();
        else
            this.nodeSet = nodeSet;
    }

    public void AddNode(GraphNode<T> node)
    {
        // adds a node to the graph
        nodeSet.Add(node);
    }

    public void AddNode(T value)
    {
        // adds a node to the graph
        nodeSet.Add(new GraphNode<T>(value));
    }

    public void AddDirectedEdge(GraphNode<T> from, GraphNode<T> to, int cost)
    {
        from.Neighbors.Add(to);
        from.Costs.Add(cost);
    }

    public void AddDirectedEdge(T from, T to)
    {
        nodeSet.FindByValue(from).Neighbors.Add(nodeSet.FindByValue(to));
    }

    public void AddUndirectedEdge(GraphNode<T> from, GraphNode<T> to, int cost)
    {
        from.Neighbors.Add(to);
        from.Costs.Add(cost);

        to.Neighbors.Add(from);
        to.Costs.Add(cost);
    }

    public void AddUndirectedEdge(T from, T to)
    {
        nodeSet.FindByValue(from).Neighbors.Add(nodeSet.FindByValue(to));
        nodeSet.FindByValue(to).Neighbors.Add(nodeSet.FindByValue(from));
    }

    public bool Contains(T value)
    {
        return nodeSet.FindByValue(value) != null;
    }

    public bool Remove(T value)
    {
        // first remove the node from the nodeset
        GraphNode<T> nodeToRemove = (GraphNode<T>)nodeSet.FindByValue(value);
        if (nodeToRemove == null)
            // node wasn't found
            return false;

        // otherwise, the node was found
        nodeSet.Remove(nodeToRemove);

        // enumerate through each node in the nodeSet, removing edges to this node
        foreach (GraphNode<T> gnode in nodeSet.list)
        {
            int index = gnode.Neighbors.IndexOf(nodeToRemove);
            if (index != -1)
            {
                // remove the reference to the node and associated cost
                gnode.Neighbors.RemoveAt(index);
                gnode.Costs.RemoveAt(index);
            }
        }

        return true;
    }

    public NodeList<T> Nodes
    {
        get
        {
            return nodeSet;
        }
    }

    public int Count
    {
        get { return nodeSet.Count; }
    }
}
