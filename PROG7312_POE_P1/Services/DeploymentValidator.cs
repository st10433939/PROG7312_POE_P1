using PROG7312_POE_P1.Models;

namespace PROG7312_POE_P1.Services
{
    public static class DeploymentValidator
    {
        public static DeploymentValidationResult Validate(
            DeploymentNode root)
        {
            var result =
                new DeploymentValidationResult();


            // Start recursive validation
            ValidateNode(
                root,
                string.Empty,
                result.Errors);


            result.IsValid =
                result.Errors.Count == 0;


            return result;
        }


        private static void ValidateNode(
            DeploymentNode node,
            string parentPath,
            List<string> errors)
        {
            // BASE CASE / VALIDATION

            string currentPath =
                string.IsNullOrWhiteSpace(parentPath)
                ? node.Name
                : $"{parentPath} -> {node.Name}";


            if (string.IsNullOrWhiteSpace(node.Name))
            {
                errors.Add(
                    $"A deployment node at {parentPath} " +
                    "does not have a name.");
            }


            if (string.IsNullOrWhiteSpace(node.Type))
            {
                errors.Add($"{currentPath} does not have a hydroponic node type specified.");
            }

            if (node.Type.Equals("Reservoir", StringComparison.OrdinalIgnoreCase) && !node.IsConfigured)
            {
                errors.Add($"{currentPath} (Reservoir) requires EC/pH calibration before deployment.");
            }

            if (!node.IsConfigured)
            {
                errors.Add(
                    $"{currentPath} is not safely configured.");
            }


            // RECURSIVE CASE
            foreach (var child in node.Children)
            {
                ValidateNode(
                    child,
                    currentPath,
                    errors);
            }
        }
    }
}