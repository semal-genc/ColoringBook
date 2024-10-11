using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBrush : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] MeshFilter meshFilterPrefab;

    [Header("Settings")]
    [SerializeField] float brushSize;
    [SerializeField] float distanceThreshold;
    Vector2 lastClikedPosition;
    private Mesh mesh;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0) 
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began) 
            {
                CreateMesh();
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Paint();
            }
        }
    }


    void CreateMesh()
    {
        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity);

        if (hit.collider == null)
        {
            return;
        }

        mesh = new Mesh();
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        vertices.Add(hit.point + (Vector3.up + Vector3.right) * brushSize / 2);
        vertices.Add(vertices[0] + Vector3.down * brushSize);
        vertices.Add(vertices[1] + Vector3.left * brushSize);
        vertices.Add(vertices[2] + Vector3.up * brushSize);

        uvs.Add(Vector2.one);
        uvs.Add(Vector2.right);
        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.up);

        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);


        triangles.Add(0);
        triangles.Add(2);
        triangles.Add(3);


        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        float zPosition = transform.childCount * .01f;
        Vector3 position = Vector3.back * zPosition;

        MeshFilter meshFilterInstance = Instantiate(meshFilterPrefab, position, Quaternion.identity, transform);
        meshFilterInstance.sharedMesh = mesh;

        meshFilterInstance.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();

        lastClikedPosition = Input.mousePosition;
    }

    void Paint()
    {
        if (Vector2.Distance(lastClikedPosition, Input.mousePosition) < distanceThreshold)
        {
            return;
        }

        lastClikedPosition = Input.mousePosition;

        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity);

        if (hit.collider == null)
        {
            return;
        }

        int startIndex = mesh.vertices.Length;

        vertices.Add(hit.point + (Vector3.up + Vector3.right) * brushSize / 2);
        vertices.Add(vertices[startIndex + 0] + Vector3.down * brushSize);
        vertices.Add(vertices[startIndex + 1] + Vector3.left * brushSize);
        vertices.Add(vertices[startIndex + 2] + Vector3.up * brushSize);

        uvs.Add(Vector2.one);
        uvs.Add(Vector2.right);
        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.up);

        triangles.Add(startIndex + 0);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 2);


        triangles.Add(startIndex + 0);
        triangles.Add(startIndex + 2);
        triangles.Add(startIndex + 3);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        transform.GetChild(transform.childCount - 1).GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
