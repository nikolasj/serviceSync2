using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApplication1.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net;
using EntityFramework.Extensions;

namespace ConsoleApplication1
{
    interface ISync
    {
        List<City> GetListFromDb();
        List<City> GetListWithKudago();
        int SyncListWithKudago();
        int SaveCityListToDb(List<City> listToAdd, List<City> listToDelete, List<City> listToUpdate);
        int AddCity(List<City> listToAdd);
        int UpdateCity(List<City> listToUpdate);
        void DeleteCity(List<City> listToDelete);
    }

    class CityTest : ISync
    {
        public List<City> GetListFromDb()
        {
            using (var db = new KudaGoContext())
            {
                var result = db.City.ToList();
                return result;
            }
        }

        public List<Models.City> GetListWithKudago()
        {
            List<Models.City> kugaGoList = new List<Models.City>();
            // вызов сервиса 
            // https://kudago.com/public-api/v1.3/locations/?lang=&fields=&order_by=

            string URL = "https://kudago.com/public-api/v1.3/locations/";
            string urlParameters = "?lang=&fields=&order_by=";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var dataObjects = response.Content.ReadAsAsync<IEnumerable<City>>().Result;
                foreach (var d in dataObjects)
                {
                    kugaGoList.Add(d);
                }
            }
            else
            {
                //Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return kugaGoList;
        }

        public int SyncListWithKudago()
        {
            List<Models.City> ourList = GetListFromDb();
            List<Models.City> kugaGoList = GetListWithKudago();

            List<Models.City> listToAdd = new List<City>();
            List<Models.City> listToDelete = new List<City>();
            List<Models.City> listToUpdate = new List<City>();


            foreach (var k in kugaGoList)
            {
                var city = ourList.FirstOrDefault(x => x.Name == k.Name);
                if (city == null)
                    listToAdd.Add(k);
                else
                {
                    city.Timezone = k.Timezone;
                    listToUpdate.Add(city);
                }

            }

            // обработка - логика слияния (мерджа) списков

            return SaveCityListToDb(listToAdd, listToDelete, listToUpdate);
        }

        public int SaveCityListToDb(List<Models.City> listToAdd, List<Models.City> listToDelete, List<Models.City> listToUpdate)
        {
            // записать в базу
            AddCity(listToAdd);
            UpdateCity(listToUpdate);
            //DeleteCity(listToDelete);
            return 0;
        }

        public int AddCity(List<Models.City> listToAdd)
        {
            using (var db = new KudaGoContext())
            {
                db.City.AddRange(listToAdd);
                db.SaveChanges();
            }
            return listToAdd.Count;
        }

        public int UpdateCity(List<Models.City> listToUpdate)
        {
            using (var db = new KudaGoContext())
            {
                foreach (var city in listToUpdate)
                {
                    var result = db.City
                        .Where(c => c.Name == city.Name)
                        .Update(c => new City
                        {
                            Coords = city.Coords,
                            Language = city.Language,
                            Name = city.Name,
                            Slug = city.Slug,
                            Timezone = city.Timezone
                        });
                }
                db.SaveChanges();
            }
            return 1;// убрать
        }

        public void DeleteCity(List<Models.City> listToDelete)
        {
            using (var db = new KudaGoContext())
            {
                foreach (var city in listToDelete)
                {
                    var result = db.City
                        .Where(c => c.Name == city.Name)
                        .Delete();
                }
                db.SaveChanges();
            }

        }
    }

    class DataLayer
    {
        #region City
        private static List<Models.City> GetCityListFromDb()
        {
            using (var db = new KudaGoContext())
            {
                var result = db.City.ToList();
                return result;
            }
        }

        private static List<Models.City> GetCityListWithKudago()
        {
            List<Models.City> kugaGoList = new List<Models.City>();
            // вызов сервиса 
            // https://kudago.com/public-api/v1.3/locations/?lang=&fields=&order_by=

            string URL = "https://kudago.com/public-api/v1.3/locations/";
            string urlParameters = "?lang=&fields=&order_by=";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var dataObjects = response.Content.ReadAsAsync<IEnumerable<City>>().Result;
                foreach (var d in dataObjects)
                {
                    kugaGoList.Add(d);
                }
            }
            else
            {
                //Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return kugaGoList;
        }

        public static int SyncCityListWithKudago()
        {
            List<Models.City> ourList = GetCityListFromDb();
            List<Models.City> kugaGoList = GetCityListWithKudago();

            List<Models.City> listToAdd = new List<City>();
            List<Models.City> listToDelete = new List<City>();
            List<Models.City> listToUpdate = new List<City>();


            foreach (var k in kugaGoList)
            {
                var city = ourList.FirstOrDefault(x => x.Name == k.Name);
                if (city == null)
                    listToAdd.Add(k);
                else
                {
                    city.Timezone = k.Timezone;
                    listToUpdate.Add(city);
                }

            }

            // обработка - логика слияния (мерджа) списков

            return SaveCityListToDb(listToAdd, listToDelete, listToUpdate);
        }

        private static int SaveCityListToDb(List<Models.City> listToAdd, List<Models.City> listToDelete, List<Models.City> listToUpdate)
        {
            // записать в базу
            AddCity(listToAdd);
            UpdateCity(listToUpdate);
            //DeleteCity(listToDelete);
            return 0;
        }

        private static int AddCity(List<Models.City> listToAdd)
        {
            using (var db = new KudaGoContext())
            {
                db.City.AddRange(listToAdd);
                db.SaveChanges();
            }
            return listToAdd.Count;
        }

        private static int UpdateCity(List<Models.City> listToUpdate)
        {
            using (var db = new KudaGoContext())
            {
                foreach (var city in listToUpdate)
                {
                    var result = db.City
                        .Where(c => c.Name == city.Name)
                        .Update(c => new City
                        {
                            Coords = city.Coords,
                            Language = city.Language,
                            Name = city.Name,
                            Slug = city.Slug,
                            Timezone = city.Timezone
                        });
                }
                db.SaveChanges();
            }
            return 1;// убрать
        }

        private static void DeleteCity(List<Models.City> listToDelete)
        {
            using (var db = new KudaGoContext())
            {
                foreach (var city in listToDelete)
                {
                    var result = db.City
                        .Where(c => c.Name == city.Name)
                        .Delete();
                }
                db.SaveChanges();
            }

        }

        #endregion

        #region Category

        private static List<Category> GetCategoryListFromDb()
        {
            using (var db = new KudaGoContext())
            {
                var result = db.Category.ToList();
                return result;
            }
        }

        private static List<Category> GetCategoryListWithKudago()
        {
            List<Category> kugaGoList = new List<Category>();
            // вызов сервиса 
            // https://kudago.com/public-api/v1.3/event-categories/?lang=&order_by=&fields=

            string URL = "https://kudago.com/public-api/v1.3/event-categories/";
            string urlParameters = "?lang=&order_by=&fields=";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var dataObjects = response.Content.ReadAsAsync<IEnumerable<Category>>().Result;
                foreach (var d in dataObjects)
                {
                    kugaGoList.Add(d);
                }
            }
            else
            {
                //Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return kugaGoList;
        }

        public static int SyncCategoryListWithKudago()
        {
            List<Category> ourList = GetCategoryListFromDb();
            List<Category> kugaGoList = GetCategoryListWithKudago();

            List<Category> listToAdd = new List<Category>();
            List<Category> listToDelete = new List<Category>();
            List<Category> listToUpdate = new List<Category>();


            foreach (var k in kugaGoList)
            {
                var category = ourList.FirstOrDefault(x => x.Name == k.Name);
                if (category == null)
                    listToAdd.Add(k);
                else
                {
                    category.Slug = k.Slug;
                    listToUpdate.Add(category);
                }

            }

            // обработка - логика слияния (мерджа) списков

            return SaveCategoryListToDb(listToAdd, listToDelete, listToUpdate);
        }

        private static int SaveCategoryListToDb(List<Category> listToAdd, List<Category> listToDelete, List<Category> listToUpdate)
        {
            // записать в базу
            AddCategory(listToAdd);
            UpdateCategory(listToUpdate);
            //DeleteCity(listToDelete);
            return 0;
        }

        private static int AddCategory(List<Category> listToAdd)
        {
            using (var db = new KudaGoContext())
            {
                var result = db.Category.AddRange(listToAdd);
                db.SaveChanges();
                return result.Count();
            }
        }

        private static int UpdateCategory(List<Category> listToUpdate)
        {
            using (var db = new KudaGoContext())
            {
                foreach (var item in listToUpdate)
                {
                    var result = db.Category
                        .Where(c => c.Name == item.Name)
                        .Update(c => new Category
                        {
                            //CategoryId = category.CategoryId,
                            Slug = item.Slug,
                        });
                }
                db.SaveChanges();
            }
            return listToUpdate.Count;
        }

        private static int DeleteCategory(List<Category> listToDelete)
        {
            using (var db = new KudaGoContext())
            {
                foreach (var item in listToDelete)
                {
                    var result = db.Category
                        .Where(c => c.Name == item.Name)
                        .Delete();
                }
                db.SaveChanges();
            }
            return listToDelete.Count;
        }

        #endregion

        #region Place

        private static List<Place> GetPlaceListFromDb()
        {
            using (var db = new KudaGoContext())
            {
                var result = db.Place.ToList();
                return result;
            }
        }

        private static List<Place> GetPlaceListWithKudago()
        {
            List<Place> kugaGoList = new List<Place>();
            // вызов сервиса 
            // https://kudago.com/public-api/v1.3/places/?lang=&fields=&expand=&order_by=&text_format=&ids=&location=&has_showings=&showing_since=1444385206&showing_until=1444385206&categories=airports,-anticafe&lon=&lat=&radius=

            string URL = "https://kudago.com/public-api/v1.3/places/";
            Int64 unixTimestamp = (Int64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            string urlParameters = "?showing_since=" + unixTimestamp + "&fields=tags,id,title,short_title,slug,address,location,timetable,phone,is_stub,images,description,body_text,site_url,foreign_url,coords,subway,favorites_count,comments_count,is_closed";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var dataObjects = response.Content.ReadAsAsync<PlaceResult>().Result;
                foreach (var d in dataObjects.ResultPlace)
                {
                    kugaGoList.Add(d);
                }
                // dataObjects.Next != "https://kudago.com/public-api/v1.3/places/?page=3"
                int j = 0;
                while (dataObjects.Next != null)
                {
                    urlParameters = dataObjects.Next.Replace(URL, "");
                    response = client.GetAsync(urlParameters).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        dataObjects = response.Content.ReadAsAsync<PlaceResult>().Result;
                        foreach (var d in dataObjects.ResultPlace)
                        {
                            kugaGoList.Add(d);
                        }
                    }
                    else
                    {
                        break;
                    }
                    j++;
                }
            }

            return kugaGoList;
        }

        public static int SyncPlaceListWithKudago()
        {
            List<Place> ourList = GetPlaceListFromDb();
            List<Place> kugaGoList = GetPlaceListWithKudago();

            List<Place> listToAdd = new List<Place>();
            List<Place> listToDelete = new List<Place>();
            List<Place> listToUpdate = new List<Place>();


            foreach (var k in kugaGoList)
            {
                var place = ourList.FirstOrDefault(x => x.Id == k.Id);
                if (place == null)
                    listToAdd.Add(k);
                else
                {                  
                    place.Slug = k.Slug;
                    listToUpdate.Add(place);
                }

            }

            // обработка - логика слияния (мерджа) списков

            return SavePlaceListToDb(listToAdd, listToDelete, listToUpdate);
        }

        private static int SavePlaceListToDb(List<Place> listToAdd, List<Place> listToDelete, List<Place> listToUpdate)
        {
            // записать в базу
            AddPlace(listToAdd);
            UpdatePlace(listToUpdate);
            //DeleteCity(listToDelete);
            return 0;
        }

        private static int AddPlace(List<Place> listToAdd)
        {
            using (var db = new KudaGoContext())
            {
                var result = db.Place.AddRange(listToAdd);
                db.SaveChanges();
                return result.Count();
            }
        }


        private static int UpdatePlace(List<Place> listToUpdate)
        {
            using (var db = new KudaGoContext())
            {
                foreach (var item in listToUpdate)
                {
                    var result = db.Place
                        .Where(c => c.Id == item.Id)
                        .Update(c => new Place
                        {
                            HasParkingLot = item.HasParkingLot,
                            ForeignUrl = item.ForeignUrl,
                            FavoritesCount = item.FavoritesCount,
                            Description = item.Description,
                            Count = item.Count,
                            Coords = item.Coords,
                            CommentsCount = item.CommentsCount,
                            CityId = item.CityId,
                            Address = item.Address,
                            BodyText = item.BodyText,
                            CategoriesKudaGo = item.CategoriesKudaGo,
                            Images = item.Images,
                            IsClosed = item.IsClosed,
                            IsStub = item.IsStub,
                            Location = item.Location,
                            Phone = item.Phone,
                            PlaceId = item.PlaceId,
                            ShortTitle = item.ShortTitle,
                            SiteUrl = item.SiteUrl,
                            Subway = item.Subway,
                            TagsKudaGo = item.TagsKudaGo,
                            Timetable = item.Timetable,
                            Title = item.Title,
                            Slug = item.Slug,
                        });
                }
                db.SaveChanges();
            }
            return listToUpdate.Count;

        }

        private static int DeletePlace(List<Place> listToDelete)
        {
            using (var db = new KudaGoContext())
            {

                foreach (var item in listToDelete)
                {
                    var result = db.Place
                        .Where(c => c.Id == item.Id)
                        .Delete();
                }
                db.SaveChanges();
            }
            return listToDelete.Count;
        }

        #endregion

        #region Event

        private static List<Event> GetEventListFromDb()
        {
            using (var db = new KudaGoContext())
            {
                var result = db.Event.ToList();
                return result;
            }
        }

        private static List<Event> GetEventListWithKudago()
        {
            List<Event> kugaGoList = new List<Event>();
            // вызов сервиса 
            // 

            string URL = "https://kudago.com/public-api/v1.3/events/";
            Int64 unixTimestamp = (Int64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            string urlParameters = "?expand=place&actual_since=" + unixTimestamp+ @"&lang=&fields=id,publication_date,title,short_title,description,body_text,dates,price,location,categories,tagline,age_restriction,is_free,favorites_count,comments_count,site_url,place,tags,images";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var dataObjects = response.Content.ReadAsAsync<EventResult>().Result;
                foreach (var d in dataObjects.ResultEvent)
                {
                    if (d.Dates != null)
                    {
                        if (d.Dates.Count() > 0)
                        {
                            d.DateStart = d.Dates[0].Start;
                            d.DateEnd = d.Dates[0].End;
                        }
                    }

                    kugaGoList.Add(d);
                }
                //dataObjects.Next != "https://kudago.com/public-api/v1.3/events/?actual_since=1478922044&fields=id%2Cpublication_date%2Ctitle%2Cshort_title%2Cdescription%2Cbody_text%2Cdates%2Cprice%2Cis_free%2Cfavorites_count%2Ccomments_count%2Csite_url%2Cplace%2tags&lang=&page=3"
                int j = 0;
                while (dataObjects.Next != null)
                {
                    urlParameters = dataObjects.Next.Replace(URL, "");
                    response = client.GetAsync(urlParameters).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        dataObjects = response.Content.ReadAsAsync<EventResult>().Result;
                        foreach (var d in dataObjects.ResultEvent)
                        {
                            if (d.Dates != null)
                            {
                                if (d.Dates.Count() > 0)
                                {
                                    d.DateStart = d.Dates[0].Start;
                                    d.DateEnd = d.Dates[0].End;
                                }
                            }

                            kugaGoList.Add(d);
                        }
                        //j++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return kugaGoList;
        }

        public static int SyncEventListWithKudago()
        {
            List<Event> ourList = GetEventListFromDb();
            List<Event> kugaGoList = GetEventListWithKudago();

            List<Event> listToAdd = new List<Event>();
            List<Event> listToDelete = new List<Event>();
            List<Event> listToUpdate = new List<Event>();


            foreach (var k in kugaGoList)
            {
                var ev = ourList.FirstOrDefault(x => x.Id == k.Id);
                if (ev == null)
                    listToAdd.Add(k);
                else
                {
                    ev.Images = k.Images;
                    ev.Slug = k.Slug;
                    listToUpdate.Add(k);
                }

            }

            // обработка - логика слияния (мерджа) списков

            return SaveEventListToDb(listToAdd, listToDelete, listToUpdate);
        }

        private static int SaveEventListToDb(List<Event> listToAdd, List<Event> listToDelete, List<Event> listToUpdate)
        {
            // записать в базу
            AddEvent(listToAdd);
            UpdateEvent(listToUpdate);
            //DeleteCity(listToDelete);
            return 0;
        }

        private static int AddEvent(List<Event> listToAdd)
        {
            using (var db = new KudaGoContext())
            {
                var result = db.Event.AddRange(listToAdd);
                db.SaveChanges();
                return result.Count();
            }
        }

        private static int UpdateEvent(List<Event> listToUpdate)
        {
            using (var db = new KudaGoContext())
            {
                foreach (var item in listToUpdate)
                {
                    var result = db.Event
                        .Where(c => c.Id == item.Id)
                        .Update(c => new Event
                        {
                            AgeRestriction = item.AgeRestriction,
                            BodyText = item.BodyText,
                            Categories = item.Categories,
                            CommentsCount = item.CommentsCount,
                            DateEnd = item.DateEnd,
                            DateStart = item.DateStart,
                            Dates = item.Dates,
                            Description = item.Description,
                            EventId = item.EventId,
                            FavoritesCount = item.FavoritesCount,
                            Images = item.Images,
                            IsFree = item.IsFree,
                            Location = item.Location,
                            Participants = item.Participants,
                            Place = item.Place,
                            Price = item.Price,
                            PublicationDate = item.PublicationDate,
                            ShortTitle = item.ShortTitle,
                            SiteUrl = item.SiteUrl,
                            Tagline = item.Tagline,
                            Tags = item.Tags,
                            TagsKudaGo = item.TagsKudaGo,
                            Title = item.Title,
                            Slug = item.Slug,
                        });
                }
                db.SaveChanges();
            }
            return listToUpdate.Count;
        }
        

        private static int DeleteEvent(List<Event> listToDelete)
        {
            using (var db = new KudaGoContext())
            {
                foreach (var item in listToDelete)
                {
                    var result = db.Event
                        .Where(c => c.Id == item.Id)
                        .Delete();
                }
                db.SaveChanges();
            }
            return listToDelete.Count;
        }

        #endregion

        #region Film


        private static List<Tag> AddTags(List<String> tags)
        {
            if (tags == null) return null;
            List<String> ourList = new List<String>();
            List<String> newList = new List<String>();
            List<Tag> listFromDb = new List<Tag>();
            Dictionary<string, Tag> tagsByText;

            using (var db = new KudaGoContext())
            {
                tagsByText = db.Tag.ToDictionary(tag => tag.Text);
            }

            foreach (var tag in tags)
            {
                if (!tagsByText.ContainsKey(tag))
                {
                    newList.Add(tag);
                }
            }

            using (var db = new KudaGoContext())
            {
                listFromDb = db.Tag.AddRange(newList.Select(l => new Tag { Text = l })).ToList();
                db.SaveChanges();
            }
            return listFromDb;
        }


        private static List<Film> GetFilmListFromDb()
        {
            using (var db = new KudaGoContext())
            {
                var result = db.Film.ToList();
                return result;
            }
        }

        private static List<Film> GetFilmListWithKudago()
        {
            List<Film> kugaGoList = new List<Film>();
            // вызов сервиса 
            // 

            string URL = "https://kudago.com/public-api/v1.3/movies/";
            string urlParameters = "?lang=&fields=id,publication_date,slug,title,genres,description,body_text,poster,language,running_time,budget_currency,mpaa_rating,age_restriction,stars,director,writer,awards,trailer,url,imdb_url,imdb_rating,trailer,country,images";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var dataObjects = response.Content.ReadAsAsync<FilmResult>().Result;
                foreach(var d in dataObjects.ResultFilm)
                {
                    kugaGoList.Add(d);
                }
                while (dataObjects.Next != null)
                {
                    urlParameters = dataObjects.Next.Replace(URL, "");
                    response = client.GetAsync(urlParameters).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        dataObjects = response.Content.ReadAsAsync<FilmResult>().Result;
                        foreach (var d in dataObjects.ResultFilm)
                        {
                            kugaGoList.Add(d);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                //Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return kugaGoList;
        }

        public static int SyncFilmListWithKudago()
        {
            List<Film> ourList = GetFilmListFromDb();
            List<Film> kugaGoList = GetFilmListWithKudago();

            List<Film> listToAdd = new List<Film>();
            List<Film> listToDelete = new List<Film>();
            List<Film> listToUpdate = new List<Film>();


            foreach (var k in kugaGoList)
            {
                var film = ourList.FirstOrDefault(x => x.Slug == k.Slug && x.FilmId == k.FilmId);
                if (film == null)
                    listToAdd.Add(k);
                else
                {
                    film.Slug = k.Slug;
                    listToUpdate.Add(k);
                }

            }

            // обработка - логика слияния (мерджа) списков

            return SaveFilmListToDb(listToAdd, listToDelete, listToUpdate);
        }

        private static int SaveFilmListToDb(List<Film> listToAdd, List<Film> listToDelete, List<Film> listToUpdate)
        {
            // записать в базу
            AddFilm(listToAdd);
            UpdateFilm(listToUpdate);
            //DeleteCity(listToDelete);
            return 0;
        }

        private static int AddFilm(List<Film> listToAdd)
        {
            using (var db = new KudaGoContext())
            {
                var result = db.Film.AddRange(listToAdd);
                db.SaveChanges();
                return result.Count();
            }
        }

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }

        private static DateTime UnixTimeStampToDateTime(string unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Int64.Parse(unixTimeStamp));
            return dtDateTime;
        }

        private static int UpdateFilm(List<Film> listToUpdate)
        {
            using (var db = new KudaGoContext())
            {
                foreach (var item in listToUpdate)
                {
                    var result = db.Film
                        .Where(c => c.FilmId == item.FilmId && c.Slug == item.Slug)
                        .Update(c => new Film
                        {
                            AgeRestriction = item.AgeRestriction,
                            Awards = item.Awards,
                            FavoritesCount = item.FavoritesCount,
                            Director = item.Director,
                            BodyText = item.BodyText,
                            Budget = item.Budget,
                            BudgetCurrency = item.BudgetCurrency,
                            CommentsCount = item.CommentsCount,
                            Country = item.Country,
                            Description = item.Description,
                            Genres = item.Genres,
                            Images = item.Images,
                            ImdbRating = item.ImdbRating,
                            ImdbUrl = item.ImdbUrl,
                            IsEditorsChoice = item.IsEditorsChoice,
                            KudaGoId = item.KudaGoId,
                            Language = item.Language,
                            Locale = item.Locale,
                            MpaaRating = item.MpaaRating,
                            OriginalTitle = item.OriginalTitle,
                            Poster = item.Poster,
                            PublicationDate = item.PublicationDate,
                            RunningTime = item.RunningTime,
                            SiteUrl = item.SiteUrl,
                            Stars = item.Stars,
                            TagsKudaGo = item.TagsKudaGo,
                            Title = item.Title,
                            Trailer = item.Trailer,
                            Url = item.Url,
                            Writer = item.Writer,
                            Year = item.Year,
                            Slug = item.Slug,
                        });
                }
                db.SaveChanges();
            }
            return listToUpdate.Count;
        }
          
        private static int DeleteFilm(List<Film> listToDelete)
        {
            using (var db = new KudaGoContext())
            {
                foreach (var item in listToDelete)
                {
                    var result = db.Film
                        .Where(c => c.Slug == item.Slug)
                        .Delete();
                }
                db.SaveChanges();
            }
            return listToDelete.Count;
        }

        #endregion

        #region Tag

        private static List<Tag> GetTagListFromDb()
        {
            using (var db = new KudaGoContext())
            {
                var result = db.Tag.ToList();
                return result;
            }
        }

        #endregion

        #region Show


        private static List<Show> GetShowListFromDb()
        {
            using (var db = new KudaGoContext())
            {
                var result = db.Show.ToList();
                return result;
            }
        }

        private static List<Show> GetShowListWithKudago()
        {
            List<Show> kugaGoList = new List<Show>();
            // вызов сервиса 
            // https://kudago.com/public-api/v1.3/locations/?lang=&fields=&order_by=

            string URL = "https://kudago.com/public-api/v1.3/movie-showings/";
            Int64 unixTimestamp = (Int64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string urlParameters = "?actual_since="+unixTimestamp+"&fields=id,movie,place,datetime,three_d,imax,four_dx,original_language,price";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var dataObjects = response.Content.ReadAsAsync<ShowResult>().Result;
                foreach (var d in dataObjects.ResultShow)
                {
                    kugaGoList.Add(d);
                }
                // dataObjects.Next != null
                //int j = 0;
                while (dataObjects.Next != null)
                {
                    urlParameters = dataObjects.Next.Replace(URL, "");
                    response = client.GetAsync(urlParameters).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        dataObjects = response.Content.ReadAsAsync<ShowResult>().Result;
                        foreach (var d in dataObjects.ResultShow)
                        {
                            kugaGoList.Add(d);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                //Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return kugaGoList;
        }

        public static int SyncShowListWithKudago()
        {
            List<Show> ourList = GetShowListFromDb();
            List<Show> kugaGoList = GetShowListWithKudago();

            List<Show> listToAdd = new List<Show>();
            List<Show> listToDelete = new List<Show>();
            List<Show> listToUpdate = new List<Show>();


            foreach (var k in kugaGoList)
            {
                var show = ourList.FirstOrDefault(x => x.ShowId == k.ShowId);
                if (show == null)
                    listToAdd.Add(k);
                else
                {
                    listToUpdate.Add(show);
                }

            }

            // обработка - логика слияния (мерджа) списков

            return SaveShowListToDb(listToAdd, listToDelete, listToUpdate);
        }

        private static int SaveShowListToDb(List<Show> listToAdd, List<Show> listToDelete, List<Show> listToUpdate)
        {
            // записать в базу
            AddShow(listToAdd);
            UpdateShow(listToUpdate);
            //DeleteCity(listToDelete);
            return 0;
        }

        private static int AddShow(List<Show> listToAdd)
        {

            using (var db = new KudaGoContext())
            {
                var result = db.Show.AddRange(listToAdd);
                db.SaveChanges();
                return result.Count();
            }
        }

        private static int UpdateShow(List<Show> listToUpdate)
        {
            using (var db = new KudaGoContext())
            {
                foreach (var item in listToUpdate)
                {
                    var result = db.Show
                        .Where(c => c.KudagoId == item.KudagoId)
                        .Update(c => new Show
                        {
                            Datetime = item.Datetime,
                            Imax = item.Imax,
                            FourDx = item.FourDx,
                            DatetimeKudago = item.DatetimeKudago,
                            MovieId = item.MovieId,
                            OriginalLanguage = item.OriginalLanguage,
                            PlaceId = item.PlaceId,
                            Price = item.Price,
                            ShowId = item.ShowId,
                            ThreeD = item.ThreeD
                        });
                }
                db.SaveChanges();
            }
            return listToUpdate.Count;
        }

        private static int DeleteShow(List<Show> listToDelete)
        {
            using (var db = new KudaGoContext())
            {
                foreach (var item in listToDelete)
                {
                    var result = db.Show
                        .Where(c => c.KudagoId == item.KudagoId)
                        .Delete();
                }
                db.SaveChanges();
            }
            return listToDelete.Count;
        }

        #endregion
    }
}
