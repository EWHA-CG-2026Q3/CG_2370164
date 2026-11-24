using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 기말 장면 세팅을 메뉴 한 번으로 해 주는 에디터 도구 (메뉴: Tools > Final)
// Final_Bar 씬이 열려 있을 때만 동작한다
public static class Final_SetupTool
{
    const string MatDir = "Assets/Final/Materials";

    // ───────────── 1. 와인잔, 셰이커, 컵받침 ─────────────
    [MenuItem("Tools/Final/1. 와인잔, 셰이커, 컵받침 만들기")]
    static void MakeProps()
    {
        if (!CheckScene()) return;

        Shader glass = Shader.Find("CG/Final_Glass");
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        if (glass == null) { Debug.LogError("CG/Final_Glass 셰이더를 찾을 수 없어요. Console 에러를 확인하세요."); return; }

        // 재질
        Material fullMat = GetOrCreateMat("Mat_Final_WineGlass", glass);
        SetWineGlass(fullMat, 0.12f);
        Material emptyMat = GetOrCreateMat("Mat_Final_WineGlassEmpty", glass);
        SetWineGlass(emptyMat, 0.08f);

        Material metal = GetOrCreateMat("Mat_Final_ShakerMetal", lit);
        metal.SetColor("_BaseColor", new Color(0.8f, 0.8f, 0.82f));
        metal.SetFloat("_Metallic", 0.9f);
        metal.SetFloat("_Smoothness", 0.85f);
        metal.SetFloat("_Cull", 0f);   // 위가 뚫려 있어서 안쪽도 보이게
        EditorUtility.SetDirty(metal);

        Material cork = GetOrCreateMat("Mat_Final_Coaster", lit);
        cork.SetColor("_BaseColor", new Color(0.35f, 0.22f, 0.12f));
        cork.SetFloat("_Smoothness", 0.1f);
        EditorUtility.SetDirty(cork);
        AssetDatabase.SaveAssets();

        // 오브젝트
        Transform root = FindOrCreate("FinalProps", null).transform;
        root.position = Vector3.zero;

        const float counterTop = 1.1f;
        const float coasterH = 0.006f;
        MakeCoaster(root, "Coaster_1", new Vector3(0.6f, counterTop, 1.4f), coasterH, cork);
        MakeCoaster(root, "Coaster_2", new Vector3(0.9f, counterTop, 1.55f), coasterH, cork);
        MakeGlass(root, "WineGlass_Full", new Vector3(0.6f, counterTop + coasterH, 1.4f), fullMat);
        MakeGlass(root, "WineGlass_Empty", new Vector3(0.9f, counterTop + coasterH, 1.55f), emptyMat);
        MakeShaker(root, "Shaker", new Vector3(-0.5f, counterTop, 1.45f), metal);

        MarkDirty();
        Debug.Log("와인잔 2개, 컵받침 2개, 셰이커를 만들었어요. Ctrl+S로 저장하세요.");
    }

    static void SetWineGlass(Material m, float fill)
    {
        m.SetColor("_BaseColor", new Color(0.87f, 0.9f, 0.93f));
        m.SetFloat("_Alpha", 0.15f);
        m.SetColor("_LiquidColor", new Color(0.43f, 0.06f, 0.12f));
        m.SetFloat("_LiquidAlpha", 0.9f);
        m.SetFloat("_FillHeight", fill);
        m.SetFloat("_FillBottom", 0.08f);
        m.SetFloat("_Cull", 0f);
        m.SetFloat("_Shininess", 160f);
        m.SetFloat("_Ambient", 0.2f);
        EditorUtility.SetDirty(m);
    }

    static void MakeGlass(Transform parent, string name, Vector3 pos, Material mat)
    {
        GameObject go = FindOrCreate(name, parent);
        Undo.RecordObject(go.transform, "Glass");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 1.5f;

        Final_WineGlass wg = GetOrAdd<Final_WineGlass>(go);
        wg.Rebuild();
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;

        // 클릭 판정용 충돌체 (잔 크기에 맞춘 캡슐)
        CapsuleCollider col = GetOrAdd<CapsuleCollider>(go);
        col.center = new Vector3(0f, 0.085f, 0f);
        col.radius = 0.05f;
        col.height = 0.17f;

        Final_ClickFill cf = GetOrAdd<Final_ClickFill>(go);
        cf.fullHeight = 0.135f;
        cf.emptyHeight = 0.08f;
    }

    static void MakeShaker(Transform parent, string name, Vector3 pos, Material mat)
    {
        GameObject go = FindOrCreate(name, parent);
        Undo.RecordObject(go.transform, "Shaker");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 1.5f;

        // 와인잔과 같은 회전 곡면 코드에 제어점만 바꿔서 셰이커 모양을 만듦
        Final_WineGlass wg = GetOrAdd<Final_WineGlass>(go);
        Undo.RecordObject(wg, "Shaker shape");
        wg.baseRadius = 0.032f;
        wg.baseHeight = 0.003f;
        wg.stemRadius = 0.032f;
        wg.stemHeight = 0.12f;
        wg.bowlP1 = new Vector2(0.036f, 0.15f);
        wg.bowlP2 = new Vector2(0.03f, 0.17f);
        wg.bowlP3 = new Vector2(0.012f, 0.2f);
        wg.Rebuild();
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;

        GetOrAdd<Final_Shake>(go);
    }

    static void MakeCoaster(Transform parent, string name, Vector3 pos, float height, Material mat)
    {
        Transform t = parent.Find(name);
        GameObject go;
        if (t != null) go = t.gameObject;
        else
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            Undo.RegisterCreatedObjectUndo(go, "Coaster");
            go.transform.SetParent(parent, false);
            Collider c = go.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);
        }
        go.transform.position = pos + Vector3.up * height * 0.5f;
        go.transform.localScale = new Vector3(0.1f, height * 0.5f, 0.1f);   // 기본 Cylinder 높이 2
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    // ───────────── 2. 갤러리 워크 코멘트 반영 ─────────────
    [MenuItem("Tools/Final/2. 코멘트 반영 (펜던트, 바닥, 카운터, 스툴, 병 모양) - 한 번만")]
    static void ApplyFeedback()
    {
        if (!CheckScene()) return;
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");

        // (1) 펜던트 조명: 높이를 조금씩 다르게, 더 밝고 넓게
        GameObject lamp = GameObject.Find("Lamp");
        if (lamp != null)
        {
            float[] heights = { 2.75f, 2.5f, 2.65f };
            int i = 0;
            foreach (Transform child in lamp.transform)
            {
                Undo.RecordObject(child, "Pendant");
                Vector3 p = child.position;
                p.y = heights[i % heights.Length];
                child.position = p;
                foreach (Light l in child.GetComponentsInChildren<Light>())
                {
                    Undo.RecordObject(l, "Pendant light");
                    l.intensity *= 1.6f;
                    l.range = Mathf.Max(l.range, 4f);
                }
                i++;
            }
        }
        else Debug.LogWarning("Lamp 오브젝트를 못 찾아서 펜던트는 건너뛰었어요.");

        // (2) 바닥: 눈에 덜 띄는 어두운 나무
        GameObject floor = GameObject.Find("Floor");
        if (floor != null)
        {
            Material m = GetOrCreateMat("Mat_Final_FloorWood", lit);
            Texture2D wood = FindTexture("Table_Wood");
            Texture2D woodN = FindTexture("Table_Wood_n", true);
            if (wood != null) m.SetTexture("_BaseMap", wood);
            if (woodN != null) { m.SetTexture("_BumpMap", woodN); m.EnableKeyword("_NORMALMAP"); }
            m.SetTextureScale("_BaseMap", new Vector2(4f, 3f));
            m.SetColor("_BaseColor", new Color(0.5f, 0.42f, 0.38f));
            m.SetFloat("_Smoothness", 0.35f);
            EditorUtility.SetDirty(m);
            SetMat(floor, m);
        }

        // (3) 카운터: 매끈하게 (바닥과 대비)
        GameObject counter = GameObject.Find("Counter");
        if (counter != null)
        {
            Material src = counter.GetComponent<Renderer>().sharedMaterial;
            Material m = GetOrCreateCopy("Mat_Final_Counter", src, lit);
            m.SetColor("_BaseColor", new Color(0.32f, 0.18f, 0.1f));
            m.SetFloat("_Smoothness", 0.8f);
            EditorUtility.SetDirty(m);
            SetMat(counter, m);
        }

        // (4) 스툴: 어두워서 안 보인다는 코멘트 → 좌석을 밝은 빨강 가죽으로
        GameObject stools = GameObject.Find("Stools");
        if (stools != null)
        {
            Renderer first = stools.GetComponentInChildren<Renderer>();
            Material m = GetOrCreateCopy("Mat_Final_Leather", first != null ? first.sharedMaterial : null, lit);
            m.SetColor("_BaseColor", new Color(0.8f, 0.16f, 0.16f));
            m.SetFloat("_Smoothness", 0.55f);
            EditorUtility.SetDirty(m);
            foreach (Renderer r in stools.GetComponentsInChildren<Renderer>())
                SetMat(r.gameObject, m);
        }

        // (5) 병 모양 다양하게: 옆모습 목록을 병마다 다르게
        Vector2[][] shapes =
        {
            null,   // 원래 모양 유지
            new[] { new Vector2(0.04f, 0f), new Vector2(0.04f, 0.24f), new Vector2(0.035f, 0.28f), new Vector2(0.016f, 0.33f),
                    new Vector2(0.015f, 0.42f), new Vector2(0.018f, 0.43f), new Vector2(0.018f, 0.45f) },            // 와인병
            new[] { new Vector2(0.055f, 0f), new Vector2(0.058f, 0.2f), new Vector2(0.05f, 0.23f), new Vector2(0.022f, 0.25f),
                    new Vector2(0.02f, 0.31f), new Vector2(0.024f, 0.32f), new Vector2(0.024f, 0.34f) },             // 위스키병
            new[] { new Vector2(0.05f, 0f), new Vector2(0.065f, 0.08f), new Vector2(0.06f, 0.16f), new Vector2(0.035f, 0.21f),
                    new Vector2(0.015f, 0.25f), new Vector2(0.015f, 0.33f), new Vector2(0.019f, 0.34f) },            // 리큐어병
        };
        Bottle[] bottles = Object.FindObjectsByType<Bottle>(FindObjectsSortMode.None);
        System.Array.Sort(bottles, (a, b) => string.CompareOrdinal(a.name, b.name));
        for (int i = 0; i < bottles.Length; i++)
        {
            Vector2[] shape = shapes[i % shapes.Length];
            if (shape == null) continue;
            Undo.RecordObject(bottles[i], "Bottle shape");
            bottles[i].profile = (Vector2[])shape.Clone();
            EditorUtility.SetDirty(bottles[i]);
            bottles[i].SendMessage("Build", SendMessageOptions.DontRequireReceiver);
        }

        AssetDatabase.SaveAssets();
        MarkDirty();
        Debug.Log("코멘트 반영 완료: 펜던트 높이·밝기, 바닥, 카운터, 스툴, 병 모양. Ctrl+S로 저장하세요.");
    }

    // ───────────── 3. 테이블 2개 (의자, 와인잔, 테이블 조명) ─────────────
    [MenuItem("Tools/Final/3. 테이블 2개 만들기")]
    static void MakeTables()
    {
        if (!CheckScene()) return;
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");

        Material wood = GetOrCreateMat("Mat_Final_TableWood", lit);
        wood.SetColor("_BaseColor", new Color(0.28f, 0.16f, 0.09f));
        wood.SetFloat("_Smoothness", 0.6f);
        EditorUtility.SetDirty(wood);

        Material leather = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "/Mat_Final_Leather.mat");
        if (leather == null)
        {
            leather = GetOrCreateMat("Mat_Final_Leather", lit);
            leather.SetColor("_BaseColor", new Color(0.8f, 0.16f, 0.16f));
            leather.SetFloat("_Smoothness", 0.55f);
            EditorUtility.SetDirty(leather);
        }
        Material wine = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "/Mat_Final_WineGlass.mat");
        AssetDatabase.SaveAssets();

        Transform root = FindOrCreate("Tables", null).transform;
        root.position = Vector3.zero;

        MakeTableSet(root, "TableSet_L", new Vector3(-3.2f, 0f, -0.4f), wood, leather, wine);
        MakeTableSet(root, "TableSet_R", new Vector3(3.2f, 0f, -0.4f), wood, leather, wine);

        MarkDirty();
        Debug.Log("테이블 2개(의자 4개, 와인잔, 테이블 조명)를 만들었어요. Ctrl+S로 저장하세요.");
    }

    static void MakeTableSet(Transform root, string name, Vector3 center, Material wood, Material leather, Material wine)
    {
        Transform set = FindOrCreate(name, root).transform;
        Undo.RecordObject(set, "Table set");
        set.position = center;

        // 테이블: 바 스툴과 같은 n각기둥 코드(BarStool)에 크기만 바꿔서 원형 테이블로
        GameObject table = FindOrCreate("Table", set);
        table.transform.localPosition = Vector3.zero;
        BarStool t = GetOrAdd<BarStool>(table);
        Undo.RecordObject(t, "Table shape");
        t.segments = 32;
        t.baseRadius = 0.35f; t.baseHeight = 0.03f;
        t.poleRadius = 0.06f; t.poleHeight = 0.75f;
        t.seatRadius = 0.65f; t.seatHeight = 0.05f;    // 좌석 → 테이블 상판
        table.GetComponent<MeshRenderer>().sharedMaterial = wood;
        table.SendMessage("Build", SendMessageOptions.DontRequireReceiver);

        // 의자 2개: 같은 코드로 낮은 의자
        for (int i = 0; i < 2; i++)
        {
            GameObject chair = FindOrCreate("Chair_" + i, set);
            chair.transform.localPosition = new Vector3(i == 0 ? -1.0f : 1.0f, 0f, 0f);
            BarStool c = GetOrAdd<BarStool>(chair);
            Undo.RecordObject(c, "Chair shape");
            c.segments = 24;
            c.baseRadius = 0.16f; c.baseHeight = 0.02f;
            c.poleRadius = 0.025f; c.poleHeight = 0.4f;
            c.seatRadius = 0.19f; c.seatHeight = 0.05f;
            chair.GetComponent<MeshRenderer>().sharedMaterial = leather;
            chair.SendMessage("Build", SendMessageOptions.DontRequireReceiver);
        }

        // 테이블 위 와인잔
        if (wine != null)
        {
            GameObject glass = FindOrCreate("WineGlass", set);
            glass.transform.localPosition = new Vector3(0.2f, 0.83f, -0.1f);
            glass.transform.localScale = Vector3.one * 1.5f;
            Final_WineGlass wg = GetOrAdd<Final_WineGlass>(glass);
            wg.Rebuild();
            glass.GetComponent<MeshRenderer>().sharedMaterial = wine;
            CapsuleCollider col = GetOrAdd<CapsuleCollider>(glass);
            col.center = new Vector3(0f, 0.085f, 0f);
            col.radius = 0.05f;
            col.height = 0.17f;
            GetOrAdd<Final_ClickFill>(glass);
        }

        // 테이블 위 작은 조명 (펜던트에서 멀어서 어둡기 때문)
        GameObject lightGo = FindOrCreate("TableLight", set);
        lightGo.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        Light l = GetOrAdd<Light>(lightGo);
        Undo.RecordObject(l, "Table light");
        l.type = LightType.Point;
        l.color = new Color(1f, 0.69f, 0.38f);
        l.range = 3f;
        l.intensity = 1.5f;
        l.shadows = LightShadows.None;
    }

    // ───────────── 도우미 ─────────────
    static bool CheckScene()
    {
        string sceneName = EditorSceneManager.GetActiveScene().name;
        if (sceneName != "Final_Bar")
        {
            EditorUtility.DisplayDialog("Final 도구", "Final_Bar 씬을 열고 실행하세요. (지금 씬: " + sceneName + ")", "확인");
            return false;
        }
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Final 도구", "Play를 끄고 실행하세요.", "확인");
            return false;
        }
        return true;
    }

    static Material GetOrCreateMat(string name, Shader shader)
    {
        string path = MatDir + "/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(shader);
            AssetDatabase.CreateAsset(m, path);
        }
        else if (m.shader != shader) m.shader = shader;
        return m;
    }

    static Material GetOrCreateCopy(string name, Material src, Shader fallback)
    {
        string path = MatDir + "/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = src != null ? new Material(src) : new Material(fallback);
            AssetDatabase.CreateAsset(m, path);
        }
        return m;
    }

    static Texture2D FindTexture(string exactName, bool asNormalMap = false)
    {
        foreach (string guid in AssetDatabase.FindAssets(exactName + " t:Texture2D"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) != exactName) continue;
            if (asNormalMap)
            {
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti != null && ti.textureType != TextureImporterType.NormalMap)
                {
                    ti.textureType = TextureImporterType.NormalMap;
                    ti.SaveAndReimport();
                }
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        Debug.LogWarning(exactName + " 텍스처를 못 찾았어요.");
        return null;
    }

    static GameObject FindOrCreate(string name, Transform parent)
    {
        Transform found = parent != null ? parent.Find(name) : null;
        if (found == null && parent == null)
        {
            GameObject g = GameObject.Find(name);
            if (g != null) found = g.transform;
        }
        if (found != null) return found.gameObject;

        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        if (c == null) c = Undo.AddComponent<T>(go);
        return c;
    }

    static void SetMat(GameObject go, Material m)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r == null) return;
        Undo.RecordObject(r, "Material");
        r.sharedMaterial = m;
    }

    static void MarkDirty()
    {
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}
