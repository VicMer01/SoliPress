using System.ComponentModel.DataAnnotations;

namespace DocumentApprovalSystem.Models
{
    public enum ApprovalMode
    {
        Majority,
        Unanimous,
        MinVotes,
        MinPercentage
    }

    public class ApprovalConfig
    {
        public int Id { get; set; }

        public ApprovalMode Mode { get; set; } = ApprovalMode.Majority;

        // Used for MinVotes or MinPercentage
        // For MinVotes: integer count
        // For MinPercentage: 0-100
        public int ThresholdValue { get; set; }

        // Whether comments are required when voting
        public bool CommentsRequired { get; set; } = false;
    }
}
