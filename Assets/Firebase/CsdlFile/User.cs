using Newtonsoft.Json;
using UnityEngine;

public class User 
{
    public string Username {get; set;}
    public int DataGold {get; set;}
    public int DataSet  {get; set;}
    public int DataCard {get; set;}
    
    public int DataCardSpin {get; set;}
    
    public int DataTicket {get; set;}
    public int Health {get; set;}
    public int Damage {get; set;}

    public bool mode;

   

    public User()
    {
      
    }

    public User(string username, int dataGold, int dataset, int dataCard,int dataCardspin,int datatikcket, int health, int damage)
    {
        Username = username;
        DataGold = dataGold;
        DataSet = dataset;
        DataCard = dataCard;
        DataCardSpin = dataCardspin;
        DataTicket = datatikcket;
        Health = health;
        Damage = damage;

      mode = false;
    }


    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}