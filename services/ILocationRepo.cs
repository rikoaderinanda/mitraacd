using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using mitraacd.Models;
using Newtonsoft.Json;

namespace mitraacd.Services
{
    public interface ILocationRepo
    {
        Task<IEnumerable<dynamic?>> NearbyKecamatan(double lat, double lng, int radiusKm);
        Task<bool> simpanCoverageArea(ReqsimpanCoverageArea data);
    }
    public class LocationRepo : ILocationRepo
    {
        private readonly IDbConnection _db;

        public LocationRepo(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<dynamic?>> NearbyKecamatan(double lat, double lng, int radiusKm)
        {
            var sql = @"SELECT 
                            name_1 AS provinsi,
                            name_2 AS kabupaten,
                            name_3 AS kecamatan,
                            round(
                                (ST_Distance(
                                    geom::geography,
                                    ST_SetSRID(ST_MakePoint(@lng, @lat), 4326)::geography
                                ) / 1000)::numeric, 2
                            ) AS jarak_km,
                            ROUND(ST_X(ST_Centroid(geom))::numeric, 6) AS longitude,
                            ROUND(ST_Y(ST_Centroid(geom))::numeric, 6) AS latitude
                        FROM kecamatan
                        WHERE ST_DWithin(
                            geom::geography,
                            ST_SetSRID(ST_MakePoint(@lng, @lat), 4326)::geography,
                            (@radiusKm*1000)
                        )
                        ORDER BY jarak_km ASC;
                        ";
            var param = new
            {
                lat = lat,
                lng = lng,
                radiusKm = radiusKm
            };
            var result = await _db.QueryAsync<dynamic>(sql, param);
            return JsonColumnParser.ParseJsonColumns(result);
        }

        public async Task<bool> simpanCoverageArea(ReqsimpanCoverageArea data)
        {
            var query = @"
                update mitra_data
                    set 
                        coverage_area = @coverage_areas::jsonb,
                        status_mitra = 3
                where id = @user_id
                returning id;
            ";

            var param = new
            {
                user_id = data.user_id,
                coverage_areas = JsonConvert.SerializeObject(data.coverage_areas)
            };

            var result = await _db.ExecuteScalarAsync<int?>(query, param);

            return result.HasValue && result.Value > 0;
        }
    }
}
