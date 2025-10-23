using Amazon.DynamoDBv2.DataModel;

namespace group_14_Munoz_Chopra__Lab_3.Models
{
    [DynamoDBTable("Comments")]
    public class Comment
    {
        [DynamoDBHashKey] 
        public int CommentID { get; set; }

        [DynamoDBRangeKey]
        public int UserID { get; set; }

        [DynamoDBProperty]
        public int EpisodeID { get; set; }

        [DynamoDBProperty]
        public int PodcastID { get; set; }

        [DynamoDBProperty]
        public string Text { get; set; }

        [DynamoDBProperty]
        public DateTime Timestamp { get; set; }
    }
}
