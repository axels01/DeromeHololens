using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Globalization;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Net;
using UnityEngine.Networking;

#if !UNITY_EDITOR && UNITY_WSA_10_0
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
#endif
/*
    Vi är bara intresserade av LWPOLYLINES som ligger i sektionen ENTITIES i DXF-filen.
    DXF structure: https://ezdxf.readthedocs.io/en/stable/dxfinternals/filestructure.html
    DXF Tags (group code, value): https://ezdxf.readthedocs.io/en/stable/dxfinternals/dxftags.html#dxf-tags-internals

    CAD/DXF-filerna innehåller endast 2D objekt (Entities) med x- och y-koordinater.
    /Madde
*/

public class Entity
{
    // Tag består av Layer name + Entity handle (se DXF Tag länk).
    private string _tag;
    public string Tag { get; set; }

    private int _vertexCount;
    public global::System.Int32 VertexCount { get => _vertexCount; set => _vertexCount = value; }

    private List<Vector3> _points;
    public List<Vector3> Points { get => _points; set => _points = value; }

    private Material _material;
    public Material Material { get => _material; set => _material = value; }
}

public class ParseDXF : MonoBehaviour
{
    private bool done = false;
    public GameObject fileSelector;
    public PressableButton bytafilen;
    public GameObject truss;
    public GameObject timber;
    public GameObject lower;
    public GameObject upper;
   // public GameObject drawing;
    public GameObject locked;
    public GameObject unlocked;
    public GameObject highlight;
    private LineRenderer lineRenderer;
    public Material[] materials = new Material[3];  //materials[0] = wood
                                                    //materials[1] = upper_plates
                                                    //materials[2] = lower_plates
    //public bool LoadOnStart = false;

    public void Clear()
    {
        foreach (Transform child in timber.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in upper.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in lower.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    void TaskOnClick(string path)
    {
        lineRenderer = GetComponent<LineRenderer>();
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        try
        {

#if !UNITY_EDITOR && UNITY_WSA_10_0
        Debug.Log("***********************************");
        Debug.Log("File Picker start.");
        Debug.Log("***********************************");


            using (StreamReader readFile = new StreamReader(path))
            {
                List<List<string>> itemList = new List<List<string>>();
                List<string> buffer = new List<string>();
                string line;
                bool flag = false;

                while ((line = readFile.ReadLine()) != null)
                {
                    // Modellerna "Virke/TIMBER" och "Anslutningspunkt/PLATE" ar LWPOLYLINES.
                    if (line.Contains("LWPOLYLINE"))
                    {
                        flag = true;
                    }
                    else if (line.Contains("LINE") || line.Contains("TEXT") || line.Contains("ARC") || line.Contains("CIRCLE"))
                    {
                        flag = false;
                    }

                    // Lagg till alla rader av LWPOLYLINES tills vi nar dess end flag ("  0").
                    // Lagg till LWPOLYLINES-rader i lista.
                    if (flag)
                    {
                        buffer.Add(line);

                        if (line.Equals("  0"))
                        {
                            if (!(buffer.Contains("Virke") || buffer.Contains("Anslutningspunkt") || buffer.Contains("TIMBER") || buffer.Contains("PLATE")))
                            {
                                buffer = new List<string>();
                                continue;
                            }
                            itemList.Add(buffer);
                            if (buffer.Contains("Anslutningspunkt") || buffer.Contains("PLATE")) { itemList.Add(buffer); }
                            buffer = new List<string>();
                        }
                        // Om vi natt slutet av ENTITIES-sektionen -> avbryt while-loop.
                        else if (line.Contains("ENDSEC")) { break; }
                    }
                }
           
                // Hantera inlast information.
                manageList(itemList); 
            }         
		
		Debug.Log("***********************************");
		Debug.Log("File Picker end.");
		Debug.Log("***********************************");
#endif
        }
        catch (Exception e)
        {
            Debug.Log("The file could not be read:");
            Debug.Log(e.Message);
        }
    }
    void Start()
    {

    }

    private void Update()
    {
        //Debug.Log("Update in ParseDXF");
        if (fileSelector.GetComponent<FileSelector>().done && !done)
        {
            done = true;
            fileSelector.SetActive(false);
            TaskOnClick(fileSelector.GetComponent<FileSelector>().selectedFile);
        }
    }

    public static string GetApi(string ApiUrl)
    {

        var responseString = "";
        var request = (HttpWebRequest)WebRequest.Create(ApiUrl);
        request.Method = "GET";
        request.ContentType = "application/json";

        using (var response1 = request.GetResponse())
        {
            using (var reader = new StreamReader(response1.GetResponseStream()))
            {
                responseString = reader.ReadToEnd();
            }
        }
        return responseString;

    }

    public void manageList(List<List<string>> list)
    {
        Entity[] entity = new Entity[list.Count];
        int plateCount = 0;

        for (int i = 0; i < list.Count; i++)
        {
            string layer = "";
            string handle = "";
            string tag = "";
            int num = 0;
            //List<Vector2> draw = new List<Vector2>();

            float x = 0, y = 0;   // Det är möjligt att float inte har tillräckligt bra precision. Kanske bör bytas till double.
            float z = 0.0f;

            // DXF-filen innehåller endast 2D modeller,
            // man får lägga till ytterligare koordinat själv och skapa Vector3(x,y,z) istället.
            List<Vector3> points = new List<Vector3>();

            // Skapa Entity objekt.
            for (int j = 0; j < list[i].Count; j++)
            {
                /*-----------------------------------------------------
                   Group codes:     num     meaning
                  -----------------------------------------------------
                                   90     num of vertices
                                    5     entity handle
                                    8     layer name
                                   10     vertex coordinate, x-value
                                   20     vertex coordinate, y-value
                -----------------------------------------------------*/
                // Utgå från Group code för att hämta och sätta värde.
                if (list[i][j].Equals(" 90")) { num = int.Parse(list[i][j + 1]); }
                else if (list[i][j].Equals("  5")) { handle = list[i][j + 1]; }
                else if (list[i][j].Equals("  8"))
                {
                    layer = list[i][j + 1];
                    tag = layer + '_' + handle;
                }
                else if (list[i][j].Equals(" 10")) { x = float.Parse(list[i][j + 1]); }
                else if (list[i][j].Equals(" 20"))
                {
                    y = float.Parse(list[i][j + 1]);

                    //Hörnpunkt Vector3(x,y,z). Byter man plats på y och z så blir modellen liggandes.
                    //Behöver dividera vektor med 1000 för att få rätt skala i Unity.
                    Vector3 point = new Vector3(x, z, y) / 1000.0f;
                    points.Add(point);
                }
            }

            if (tag.Contains("Anslutningspunkt") || tag.Contains("PLATE")) { plateCount++; }

            if ((plateCount > 0) && (plateCount % 2 == 0) && (tag.Contains("Anslutningspunkt") || tag.Contains("PLATE")))
            {
                //Undre plåt får suffixet '_Lower' och hamnar längre ner.
                entity[i] = new Entity();
                entity[i].Tag = tag + "_Lower";
                entity[i].VertexCount = num;
                entity[i].Points = points;
                for (int p = 0; p < num; p++)
                {
                    entity[i].Points[p] += new Vector3(0, -0.045f, 0);
                }
                entity[i].Material = materials[2];
            }
            else
            {
                entity[i] = new Entity();
                entity[i].Tag = tag;
                entity[i].VertexCount = num;
                entity[i].Points = points;

                if (tag.Contains("Virke") || tag.Contains("TIMBER")) { entity[i].Material = materials[0]; }
                else { entity[i].Material = materials[1]; }
            }

            /*for (int p = 0; p < num; p++)
            {
                //Debug.Log(entity[i].Points[p]);
                // Tafs tafs, dålig kod på ingång
                //draw.Add(entity[i].Points[p]);
                //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //cube.transform.position = new Vector3(points[p][0]/1000, points[p][1]/1000, 3);
                //cube.transform.localScale = new Vector3(0.01f,0.01f, 0.01f);
            }
            //Draw(draw, entity[i].Tag);
            //draw = null;*/
        }

        //Ritar upp 2D-objekt.
        //lineRender(entity);

        //Skapar 3D GameObjects. work in progress...
        PBMesh(entity);

        //Kör debug.
        //log(entity);

        //Skriv till fil.
        //writeData(entity);
    }

    //Ritar upp 2D-objekt.
    /*public void lineRender(Entity[] entity)
    {
        //'Ny' funktion. Ritar upp 2D-miniatyr på DrawingCanvas.
        GameObject canvas = GameObject.Find("DrawingPanel");

        foreach (Entity e in entity)
        {
            if (e.Tag.Contains("_Lower")) { continue; }

            GameObject obj = new GameObject();
            obj.name = e.Tag;

            lineRenderer = obj.AddComponent<LineRenderer>();
            int count = e.Points.Count;
            float width = 0.0015f;
            Vector3[] vertices = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                vertices[i].x = e.Points[i].x / 21.0f;
                vertices[i].y = e.Points[i].z / 21.0f;
                vertices[i].z = e.Points[i].y / 21.0f;
            }
            lineRenderer.loop = true;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.positionCount = count;
            lineRenderer.SetPositions(vertices);
            lineRenderer.useWorldSpace = false;

            if (e.Tag.Contains("Virke") || e.Tag.Contains("TIMBER"))
            {
                lineRenderer.material.color = Color.yellow;
            }
            else { lineRenderer.material.color = Color.red; }

            lineRenderer.transform.position = canvas.transform.position + new Vector3(0, -0.04f, -0.00005f);
            lineRenderer.transform.parent = drawing.transform;
        }
    }*/

    //----- ProBuilderMesh -------------------------------
    //Dessa snubbar får förklara vad som händer här:
    //      https://www.youtube.com/watch?v=gmuHI_wsOgI 
    //      https://www.youtube.com/watch?v=RdnfidIaGqc
    //----------------------------------------------------
    void PBMesh(Entity[] entity)
    {
        Clear();
        locked.SetActive(false);
        unlocked.SetActive(false);
        highlight.SetActive(false);
        //Skapa 3D-modell.
        foreach (Entity e in entity)
        {
            Vector3[] vertices = new Vector3[e.VertexCount];
            int i_count = (e.VertexCount - 2) * 3;      //Antal indices = ((antal vertices - 2) * 3)
            int[] triangles = new int[i_count];         //3 indices/triangel. Antal trianglar = antal vertices - 2

            for (int i = 0; i < e.VertexCount; i++)
            {
                vertices[i] = e.Points[i];
            }

            //Fult och hårdkodat, men funkar... 
            if ((e.Tag.Contains("Virke") || e.Tag.Contains("TIMBER")) && e.VertexCount == 4)
            {
                triangles[0] = 0;
                triangles[1] = 1;
                triangles[2] = 2;

                triangles[3] = 3;
                triangles[4] = 0;
                triangles[5] = 2;
            }
            else if (e.Tag.Contains("Anslutningspunkt") || e.Tag.Contains("PLATE"))
            {
                triangles[0] = 0;
                triangles[1] = 1;
                triangles[2] = 2;

                triangles[3] = 0;
                triangles[4] = 2;
                triangles[5] = 3;
            }
            else if (e.VertexCount == 5)
            {
                triangles[0] = 0;
                triangles[1] = 1;
                triangles[2] = 2;

                triangles[3] = 0;
                triangles[4] = 2;
                triangles[5] = 4;

                triangles[6] = 4;
                triangles[7] = 2;
                triangles[8] = 3;
            }
            else if (e.VertexCount == 6)
            {
                triangles[0] = 0;
                triangles[1] = 1;
                triangles[2] = 2;

                triangles[3] = 0;
                triangles[4] = 2;
                triangles[5] = 5;

                triangles[6] = 2;
                triangles[7] = 3;
                triangles[8] = 5;

                triangles[9] = 3;
                triangles[10] = 4;
                triangles[11] = 5;
            }

            Face[] faces = new Face[] { new Face(triangles) };
            //Create a new GameObject with a ProBuilderMesh component, MeshFilter, and MeshRenderer.
           ProBuilderMesh mesh = truss.GetComponent<ProBuilderMesh>();
            // mesh.Clear();
            mesh = ProBuilderMesh.Create(vertices, faces);
            //mesh.SetVertices(meshtemp.GetVertices()); //update mesh
            //meshtemp.Clear();
            mesh.name = e.Tag;
            

            if (e.Tag.Contains("Virke") || e.Tag.Contains("TIMBER"))
            {
                mesh.Extrude(faces, ExtrudeMethod.FaceNormal, 0.045f);
                mesh.transform.parent = timber.transform;
            }
            else if (e.Tag.Contains("Lower"))
            {
                mesh.Extrude(faces, ExtrudeMethod.FaceNormal, 0.005f);
                mesh.transform.parent = lower.transform;
            }
            else
            {
                mesh.Extrude(faces, ExtrudeMethod.FaceNormal, -0.005f);
                mesh.transform.parent = upper.transform;
            }

            mesh.SetMaterial(mesh.faces, e.Material); //Sätt material på samtliga faces.
            var mc = mesh.gameObject.AddComponent<MeshCollider>();
            mc.convex = true;

            mesh.ToMesh();
            mesh.Refresh();
        }

        //Flytta Truss så att modellen inte hamnar mitt framför ögonen.
        truss.transform.position += new Vector3(-0.8f, -0.5f, 0.5f);
        locked.SetActive(true);
    }

    void Draw(List<Vector2> lst, string name)
    {
        Vector3[] positionsk = new Vector3[3] { new Vector3(0, 0, 0), new Vector3(-1, 1, 0), new Vector3(1, 1, 0), };
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.loop = true;
        lineRenderer.positionCount = 3;
        lineRenderer.SetPositions(positionsk);
        Debug.Log(lst.Count);
        Debug.Log(lst[0]);
        Debug.Log(lst[1]);
        Debug.Log(lst[2]);
        Debug.Log(lst[3]);
        if (lst.Count == 4)
        {
            for (int j = 0; j < 2; j++)
            {
                Vector3 q = lst[0];
                Vector3 p = lst[j + 1];
                Vector3 q2 = lst[j + 2];
                Debug.Log(lst[0] + "k");
                Debug.Log(lst[1]);
                Debug.Log(lst[2]);
                Debug.Log(lst[3]);
                Vector3[] positions = new Vector3[3] { q, p, q2 };
                DrawTriangel(positions, 0.01f, 0.01f, name + "_" + j.ToString());
            }

        }
        else
        {
            for (int i = 0; i < lst.Count - 2; i += 3)
            {
                Vector3 q = lst[i];
                Vector3 p = lst[i + 1];
                Vector3 q2 = lst[i + 2];
                Vector3[] positions = new Vector3[3] { q, p, q2 };
                //Vector3[] positions = new Vector3[3] { lst[i], lst[i + 1], lst[i + 2] };
                DrawTriangel(positions, 0.01f, 0.01f, name + "_" + i.ToString());
            }
        }
    }

    void DrawTriangel(Vector3[] positions, float startWidth, float endWidth, string name)
    {
        GameObject obj = new GameObject();
        obj.name = name;
        lineRenderer = obj.AddComponent<LineRenderer>();
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.loop = true;
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    // Debug Entity innehåll.
    // Att tänka på vid utskrift i Unity console: Debug.Log avrundar punkterna till en decimal. ToString("F#") -> ange antal decimaler. 
    public void log(Entity[] entity)
    {
        foreach (Entity e in entity)
        {
            Debug.Log(e.Tag);
            foreach (Vector3 vect in e.Points) { Debug.Log(vect.ToString("F3")); }
        }
    }

    // Funktion för att skriva ut hittad information till fil.
    public void writeData(Entity[] entity)
    {
        using (StreamWriter writeFile = new StreamWriter("entities.txt"))
        {
            foreach (Entity e in entity)
            {
                //if (string.IsNullOrEmpty(line)) { continue; } //bör inte finnas några empty/null lines.
                writeFile.WriteLine("Layer: " + e.Tag);
                foreach (Vector3 vect in e.Points) { writeFile.WriteLine("      " + vect.ToString("F3")); }
            }
        }
    }
}