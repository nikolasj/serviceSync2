using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.Models
{
    class KudaGoContext : DbContext
    {
        public KudaGoContext() : base("KudaGoDb") 
        {
            //создаем контекст для пересоздания БД в случае изменений
            //Database.SetInitializer(new CreateDatabaseIfNotExists<KudaGoContext>());
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<KudaGoContext>());
            //Database.SetInitializer(new DropCreateDatabaseAlways<KudaGoContext>());
            //Database.SetInitializer(new KudaGoContextInitializer());
        }

        public DbSet<Category> Category { get; set; }

        public DbSet<City> City { get; set; }

        public DbSet<Comment> Comment { get; set; }

        //public DbSet<Coords> Coords { get; set; }

        //public DbSet<DateRange> DateRange { get; set; }

        public DbSet<Event> Event { get; set; }

        //public DbSet<EventResult> EventResult { get; set; }

        public DbSet<Film> Film { get; set; }

        //public DbSet<FilmResult> FilmResult { get; set; }

        public DbSet<Genres> Genres { get; set; }

        public DbSet<Images> Images { get; set; }

        //public DbSet<Location> Location { get; set; }

        public DbSet<Movie> Movie { get; set; }

        public DbSet<Opportunity> Opportunity { get; set; }

        public DbSet<Place> Place { get; set; }

        //public DbSet<PlaceResult> PlaceResult { get; set; }

        public DbSet<PlaceShow> PlaceShow { get; set; }

        public DbSet<Poster> Poster { get; set; }

        public DbSet<Route> Route { get; set; }

        public DbSet<Show> Show { get; set; }

        //public DbSet<ShowResult> ShowResult { get; set; }

        public DbSet<SocialConnection> SocialConnection { get; set; }

        //public DbSet<Source> Source { get; set; }

        public DbSet<Tag> Tag { get; set; }

        public DbSet<User> User { get; set; }

    }
}
