using MongoDB.Bson.Serialization.Attributes;

namespace SocialNetwork.DTO.Entities.Base
{
    public class ModelBase<T>
    {
        [BsonId]
        public T Id { get; set; }

        public Meta Meta { get; set; }

        public ModelBase()
        {
            Meta = new Meta();
        }
    }
}
