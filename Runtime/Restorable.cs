using System.Text.Json.Nodes;

public interface Restorable
{
    public JsonObject MakeSnapshot();
    public void Restore(JsonObject snapshot);
}
