using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// converts a 2d annotation to a 3d mesh
/// </summary>
[RequireComponent(typeof(Camera))]
public class ProjectionMapper : AManager<ProjectionMapper>
{
    #region properties
    private List<Vector3> corners = new List<Vector3>();
    /// <summary>
    /// for debugging purposes: visualize all vertices's with this geometry
    /// </summary>
    public Transform showCornersPrefab;
    /// <summary>
    /// plane to project on
    /// </summary>
    public Transform projectionPlane;
    public MeshFilter resultMesh;

    /// <summary>
    /// plane to project on
    /// </summary>
    public Transform ProjectionPlane
    {
        get
        {
            if (!projectionPlane)
            {
                projectionPlane = (new GameObject()).transform;
                projectionPlane.parent = transform;
                projectionPlane.localScale = Vector3.one;
                projectionPlane.localPosition = Vector3.zero;
                projectionPlane.localEulerAngles = Vector3.zero;
            }
            return projectionPlane;
        }
    }

    /// <summary>
    /// transform all calculated mesh corners from world space to local space
    /// </summary>
    private Vector2[] RelativCorners
    {
        get
        {
            Vector2[] relativ = new Vector2[corners.Count];
            for (int i = 0; i < corners.Count; i++)
            {
                var relativePosition = ProjectionPlane.transform.InverseTransformPoint(corners[i]);
                relativ[i] = new Vector2(relativePosition.x, relativePosition.z);
            }
            return relativ;
        }
    }

    /// <summary>
    /// uv coordinates for all mesh vertices's
    /// </summary>
    private List<Vector2> uvs = new List<Vector2>();
    private Vector2[] UVs
    {
        get
        {
            if (uvs.Count == 0)
            {
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0.5f, 0.5f));
            }
            return uvs.ToArray();
        }
    }

    private List<int> indices = new List<int>();

    /// <summary>
    /// virtual camera representing the projector that projects the 2d annotation onto the selected AR plane
    /// </summary>
    private Camera cam;
    private Camera Cam
    {
        get
        {
            if (!cam)
                cam = GetComponent<Camera>();
            return cam;
        }
    }
    #endregion

    private void Start()
    {
        if (resultMesh)
            generateNewMesh(resultMesh);
    }

    /// <summary>
    /// sync the projector settings with the AR camera
    /// </summary>
    private void syncCameraParameter()
    {
        Cam.fieldOfView = CameraHelper.ARCamera.fieldOfView;
        Cam.nearClipPlane = CameraHelper.ARCamera.nearClipPlane;
        Cam.farClipPlane = CameraHelper.ARCamera.farClipPlane;
        Cam.ResetAspect();
    }

    /// <summary>
    /// generate a 3d mesh for the 2d annotation. Generate the mesh in a separate thread.
    /// </summary>
    /// <param name="meshFilter"></param>
    /// <returns></returns>
    IEnumerator generateNewMeshAsync(MeshFilter meshFilter)
    {
        //wait one moment until the main thread prepare the basic elements (transform position and rotation for the projector and the projection plane)
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        syncCameraParameter();
        calculateCorners(50);
        generateMesh(meshFilter);
        yield return null;
    }

    /// <summary>
    /// generate a 3d mesh for the 2d annotation
    /// </summary>
    /// <param name="targetPosition">position of the projection plane</param>
    /// <param name="targetRotation">rotation of the projection plane</param>
    /// <param name="projectorPosition">position of the projector</param>
    /// <param name="projectorRotation">rotation of the projector</param>
    /// <param name="projectionTexture">annotation texture</param>
    /// <param name="anchor">connected anchor point</param>
    /// <param name="meshFilter">game object with will display the generated mesh</param>
    public void generateNewMesh(
        Vector3 targetPosition, Vector3 targetRotation, Vector3 projectorPosition, Vector3 projectorRotation,
        Texture2D projectionTexture, AnchorPoint anchor,
        MeshFilter meshFilter)
    {
        if (ProjectionPlane)
        {
            ProjectionPlane.transform.position = targetPosition;
            ProjectionPlane.transform.eulerAngles = targetRotation;
        }
        transform.position = projectorPosition;
        transform.eulerAngles = projectorRotation;
        generateNewMesh(meshFilter);

        if (anchor.PoseDriver != null)
        {
            AnchorPointManager.Instance.ChangeReferencePointForAnchor(anchor, targetPosition, Quaternion.Euler(targetRotation));
        }
    }

    /// <summary>
    /// generate a 3d mesh for the 2d annotation
    /// </summary>
    /// <param name="meshFilter">game object with will display the generated mesh</param>
    public void generateNewMesh(MeshFilter meshFilter)
    {
        StartCoroutine(generateNewMeshAsync(meshFilter));
    }


    #region calculate mesh
    /// <summary>
    /// calculate the vertex position of the new mesh
    /// </summary>
    /// <param name="tessellation">How fine should the mesh be resolved? Number of vertices's in horizontal and vertical direction of the annotation.</param>
    private void calculateCorners(int tessellation = 1)
    {
        if (ProjectionPlane)
        {
            int childs = ProjectionPlane.transform.childCount;
            for (int i = childs - 1; i > 0; i--)
            {
                GameObject.Destroy(ProjectionPlane.transform.GetChild(i).gameObject);
            }
        }

        // create vertices's and UV's
        corners.Clear();
        uvs.Clear();
        int noPointFoundCount = 0;
        for (int i = 0; i <= tessellation; i++)
        {
            for (int j = 0; j <= tessellation; j++)
            {
                float stepI = (1f / tessellation) * i;
                float stepJ = (1f / tessellation) * j;
                var vectorPoint = calcCorner(new Vector2(stepI * Cam.pixelWidth, stepJ * Cam.pixelHeight));
                corners.Add(vectorPoint);
                if (vectorPoint == Vector3.zero)
                    noPointFoundCount++;
                uvs.Add(new Vector2(stepI, stepJ));
            }
        }

        if (noPointFoundCount>0)
        {
            Debug.Log("[Test] calcCorner not found: " + noPointFoundCount);
        }

        calculateIndices(tessellation);
    }

    /// <summary>
    /// calculate triangles
    /// </summary>
    /// <param name="tessellation">How fine should the mesh be resolved? Number of vertices's in horizontal and vertical direction of the annotation.</param>
    private void calculateIndices(int tessellation = 1)
    {
        indices.Clear();
        int rowCount = tessellation + 1;
        for (int i = 0; i < tessellation; i++)
        {
            for (int j = 0; j < tessellation; j++)
            {
                var c1 = i * rowCount + j;
                var c2 = i * rowCount + j + 1;
                var c3 = (i + 1) * rowCount + j;
                var c4 = (i + 1) * rowCount + j + 1;

                if (corners[c2] != Vector3.zero && corners[c3] != Vector3.zero)
                {
                    if (corners[c1] != Vector3.zero && corners[c2] != Vector3.zero && corners[c3] != Vector3.zero)
                    {
                        indices.Add(c1);
                        indices.Add(c2);
                        indices.Add(c3);
                    }

                    if (corners[c4] != Vector3.zero && corners[c2] != Vector3.zero && corners[c3] != Vector3.zero)
                    {
                        indices.Add(c3);
                        indices.Add(c2);
                        indices.Add(c4);
                    }
                }
                else
                {
                    if (corners[c1] != Vector3.zero && corners[c2] != Vector3.zero && corners[c4] != Vector3.zero)
                    {
                        indices.Add(c1);
                        indices.Add(c2);
                        indices.Add(c4);
                    }

                    if (corners[c4] != Vector3.zero && corners[c1] != Vector3.zero && corners[c3] != Vector3.zero)
                    {
                        indices.Add(c3);
                        indices.Add(c1);
                        indices.Add(c4);
                    }
                }
            }
        }
    }

    /// <summary>
    /// generate a 3d mesh for the calculate vertices's
    /// </summary>
    /// <param name="meshFilter">game object with will display the generated mesh</param>
    private void generateMesh(MeshFilter meshFilter)
    {
        var mesh = new Mesh();
        var relativ = RelativCorners;
        calculateMesh(mesh, relativ);
        meshFilter.mesh = mesh;
        meshFilter.transform.rotation = ProjectionPlane.rotation;
        meshFilter.transform.position = ProjectionPlane.position;
        meshFilter.transform.localScale = ProjectionPlane.localScale;
    }

    /// <summary>
    /// calculate the mesh geometry
    /// </summary>
    /// <param name="mesh">result mesh</param>
    /// <param name="convexPolygon">list of vertices's</param>
    /// <returns></returns>
    private bool calculateMesh(Mesh mesh,  Vector2[] convexPolygon)
    {
        // Vertices's
        var vertices = new List<Vector3>();
        foreach (var point2 in convexPolygon)
        {
            var point3 = new Vector3(point2.x, 0, point2.y);
            vertices.Add(point3);
        }

        mesh.Clear();
        mesh.SetVertices(vertices);

        // Indices's / Triangles
        const int subMesh = 0;
        const bool calculateBounds = true;
        mesh.SetTriangles(indices, subMesh, calculateBounds);


        // UVs
        if (UVs.Length == vertices.Count)
            mesh.uv = UVs;

        // Normals
        // Reuse the same list for normals
        var normals = vertices;
        for (int i = 0; i < normals.Count; ++i)
            normals[i] = Vector3.up;

        mesh.SetNormals(normals);

        return true;
    }

    /// <summary>
    /// Calculate the position of one vertex. The position is calculated by projecting a screen point from the projector onto the projection plane.
    /// </summary>
    /// <param name="screenPos">screen point</param>
    /// <param name="showCornerPrefab">visualize vertex for debugging purpose</param>
    /// <returns></returns>
    private Vector3 calcCorner(Vector2 screenPos, bool showCornerPrefab = true)
    {
        LayerMask mask = LayerMask.GetMask("ProjectionPlane");
        RaycastHit hit;

        Ray ray = Cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out hit, 10000000, mask))
        {
            if (showCornerPrefab && showCornersPrefab && ProjectionPlane)
            {
                var showCorner = GameObject.Instantiate(showCornersPrefab, ProjectionPlane.transform);
                showCorner.position = hit.point;
                showCorner.gameObject.SetActive(true);
            }
            return hit.point;
        }

        return Vector3.zero;
    }
    #endregion
}
