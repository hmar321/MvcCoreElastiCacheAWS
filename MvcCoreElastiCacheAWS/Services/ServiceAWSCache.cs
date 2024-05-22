using MvcCoreElastiCacheAWS.Helpers;
using MvcCoreElastiCacheAWS.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MvcCoreElastiCacheAWS.Services
{
    public class ServiceAWSCache
    {
        private IDatabase cache;
        private string idkey;

        public ServiceAWSCache()
        {
            this.cache = HelperCacheRedis.Connection.GetDatabase();
        }

        public async Task<List<Coche>> GetCochesFavoritosAsync()
        {
            //ALMACENAREMOS UNA COLECCION DE COCHES EN FORMATO JSON
            //LAS KEYS DEBEN SER UNICAS PARA CADA USER
            string jsonCoches = await this.cache.StringGetAsync(this.idkey);
            if (jsonCoches == null)
            {
                return null;
            }
            else
            {
                List<Coche> cars = JsonConvert.DeserializeObject<List<Coche>>(jsonCoches);
                return cars;
            }
        }

        public async Task AddCcocheFavoritoAsync(Coche car)
        {
            List<Coche> cars = await this.GetCochesFavoritosAsync();
            //SI NO EXISTE LA COLECCION LA CREAMOS
            if (cars == null)
            {
                cars = new List<Coche>();
            }
            //AÑADIMOS EL NUEVO COCHE A LA COLECCION
            cars.Add(car);
            string jsonCoches = JsonConvert.SerializeObject(cars);
            //ALMACENAMOS LA COLECCION DENTRO DE CACCHE REDIS
            //INDICANDO QUE LOS DATOS DURARAN 30 MIN
            await this.cache.StringSetAsync(this.idkey, jsonCoches, TimeSpan.FromMinutes(30));
        }
        public async Task DeleteCcocheFavoritoAsync(int idcoche)
        {
            List<Coche> cars = await this.GetCochesFavoritosAsync();
            if (cars != null)
            {
                Coche cocheEliminar = cars.FirstOrDefault(x => x.IdCoche == idcoche);
                cars.Remove(cocheEliminar);
                //COMPROBAMOS SI LA COLLECCIOND CARS TIENE COCHES FAVORITOS
                //SI NO TENEMOS COCHES ELIMINAMOS LA KEY DE CACHE REDIS
                if (cars.Count == 0)
                {
                    await this.cache.KeyDeleteAsync(this.idkey);
                }
                else
                {
                    //ALMACENAMOS DE NUEVO LOS COCHES SIN EL COCHE ELIMINADO
                    string jsonCoches = JsonConvert.SerializeObject(cars);
                    //ACTUALIZAMOS EL CACHE REDIS
                    await this.cache.StringSetAsync(this.idkey, jsonCoches, TimeSpan.FromMinutes(30));
                }
            }
        }
    }
}
