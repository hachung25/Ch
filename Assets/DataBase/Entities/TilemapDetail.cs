using Newtonsoft.Json;
using UnityEngine;


 public enum TilemapSate
{
    Tilemap,
    Grass,
    Forest,

}


public class TilemapDetail
{
    public int x { get; set; }

    public int y { get; set; }

    public TilemapSate tilemapSate { get; set; }

    public TilemapDetail() 
    { 

    }

    public TilemapDetail(int x, int y, TilemapSate tilemapSate)
    {
        this.x = x;
        this.y = y;
        this.tilemapSate = tilemapSate;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
