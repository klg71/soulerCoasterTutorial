using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(MeshFilter))]
public class SoulerCoasterTutorial : MonoBehaviour {
    // ContextMenus can be called through right-click on the component in the inspector
    [ContextMenu("generate()")]
    public void generate() {
        // Retrieve the path from the LineRenderer
        var positions = new Vector3[GetComponent<LineRenderer>().positionCount];
        GetComponent<LineRenderer>().GetPositions(positions);

        // Holds the vectors
        List<Vector3> vectorList = new();
        // Holds the triangle faces
        List<int> triangleList = new();
        // Holds the UV map
        List<Vector2> uvCoordinates = new();

        // First we push the vertical plane
        var nextIndex = pushQuadPlane(0, positions, vectorList, uvCoordinates, triangleList, 0);
        // Second we push the horizontal plane
        pushQuadPlane(nextIndex, positions, vectorList, uvCoordinates, triangleList, 90);

        // Create the mesh and assign it
        var mesh = new Mesh {
            name = "soulercoaster",
            vertices = vectorList.ToArray(),
            uv = uvCoordinates.ToArray(),
            triangles = triangleList.ToArray()
        };
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private static int pushQuadPlane(int startIndex, Vector3[] positions, List<Vector3> vectorList, List<Vector2> uvCoordinates,
        List<int> triangleList,
        int rotate) {
        var currentPosition = positions[0];
        var deltaDirection = positions[1] - currentPosition;
        // Find an orthogonal vector to the path and rotate it around the given rotation (0 for horizontal, 90 for vertical)
        var quadDirection = Quaternion.AngleAxis(rotate, deltaDirection) * Vector3.Cross(currentPosition, deltaDirection).normalized;

        // Push initial quads
        vectorList.Add(currentPosition + quadDirection);
        vectorList.Add(currentPosition - quadDirection);
        // Start at the bottom of the UV map
        uvCoordinates.Add(new Vector2(0, 0));
        uvCoordinates.Add(new Vector2(1, 0));
        var lastPosition = currentPosition;

        // We need the starting Index so the triangles can reference the correct vectors
        var i = startIndex + 1;

        // Iterate over all positions to create the quad
        // Each iterations adds two new vectors that build a quad with the previous two vectors.
        // That is the reason we need to push the initial quads without faces
        while (i - startIndex < positions.Length) {
            currentPosition = positions[i - startIndex];
            deltaDirection = currentPosition - lastPosition;

            // Find an orthogonal vector to the path and rotate it around the given rotation (0 for horizontal, 90 for vertical)
            quadDirection = Quaternion.AngleAxis(rotate, deltaDirection) * Vector3.Cross(currentPosition, deltaDirection).normalized;
            vectorList.Add(currentPosition + quadDirection);
            vectorList.Add(currentPosition - quadDirection);
            
            // Calculate the progress along the UV map
            var progress = (i - startIndex) / (positions.Length * 1f);
            uvCoordinates.Add(new Vector2(0, progress));
            uvCoordinates.Add(new Vector2(1, progress));

            // Build the quad face from two triangles
            // I think this is the most complicated part.
            // Each mesh consists of triangles. A triangle is defined by pushing 3 entries onto the triangle array.
            // These 3 vectors need to be in clockwise order or they will be interpreted as backfaces.
            
            // Push the lower right vector
            triangleList.Add(i * 2 - 2);
            // Push the lower left vector
            triangleList.Add(i * 2 - 1);
            // Push the upper right vector
            triangleList.Add(i * 2);

            // Push the lower left vector
            triangleList.Add(i * 2 - 1);
            // Push the upper left vector
            triangleList.Add(i * 2 + 1);
            // Push the upper right vector
            triangleList.Add(i * 2);

            lastPosition = currentPosition;
            i++;
        }

        return i;
    }
}