namespace PROG7312_POE_P1.Models
{
    public class DeploymentNode
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public bool IsConfigured { get; set; }

        public List<DeploymentNode> Children { get; set; } = new List<DeploymentNode>();
    }

    public class DeploymentValidationResult
    {
        public bool IsValid { get; set; }

        public List<string> Errors { get; set; } = new List<string>();
    }
}
