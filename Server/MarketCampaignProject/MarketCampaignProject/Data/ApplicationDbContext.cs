using System; //give all the core data to the system like int ,string etc.
using System.Collections.Generic;//to store all the different types of DS like list,dictionary,hashset
using Microsoft.EntityFrameworkCore;//Bring all the entity framework tools like dbcontext,dbset
using MarketCampaignProject.Models;
using System.Security.Cryptography.X509Certificates; // Bring the data of the model

namespace MarketCampaignProject.Data // put all the code in the folder data
{
    public class ApplicationDbContext : DbContext // its a class named ApplicationDbcontext which will have all the properties of Dbcontext 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        
            //it is the constructor for ApplicationDb context class where we will initialize the constructor
            // inside the contructor we are initializing - dbcontextoptions is the tool which contain all the data of database 
            // and in order to get the right class we are defining it there in order to reduce confusion 
            // options is the name of the setting box which we have created here like - ApplicationDbContext
            //Simple words - Ham ek constructor bnaye hai jiska name ApplicationDbcontext hai which is same
            //as the class name ie ApplicationDbcontext - is constructor mai hm kuch properties define krnge jese ki 
            // ye whole dbcontextoptions ek tool hai jiske ander sara commands hai to access database 
            // and hm ye tool ko use kr the just for our custom robot named Applicationdbcontext , so these whole 
            //constructor is like a setting spray jo initoalize krega jb object banega and is whole setting paper
            // ko ham name denge options means all the initialization here is called options
            // now base <options> means ki setting paper jo bnaye hai usko wapas robot ko de do ie Dbcontext 
            //in order to set up the data and configure( arrange )it .
            public DbSet<User> Users { get; set; }
            //Give me the magic door named Users which will open the door for user table 
            public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Lead> Leads { get; set; }

    }
}
