﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class Database : MonoBehaviour
{
    const string databaseAdress = "/Resources/Database/";
    const string databaseFolder = "Database/";

    static List<GameObject> prefabObjects = new List<GameObject>(1000);

    static Dictionary<string, Entity> entities = new Dictionary<string, Entity>(1000);
    static Dictionary<string, HashSet<string>> Components = new Dictionary<string, HashSet<string>>(10000);

    [MenuItem("URSA/Database/Rebuild")]
    public static void Rebuild()
    {
        prefabObjects = new List<GameObject>(1000);
        entities = new Dictionary<string, Entity>(1000);
        Components = new Dictionary<string, HashSet<string>>(10000);

        assign_ids_entities();
        assign_ids_components();
    }
    
    //scans the database 
    public void RebuildIDs()
    {

    }

    static void assign_ids_entities()
    {
        var files = Directory.GetFiles(Application.dataPath + "/Resources/Database/", "*.prefab", SearchOption.AllDirectories);
        foreach (var f in files)
        {
            string id = f.Remove(0, f.IndexOf(databaseAdress)+ databaseAdress.Length);
            id = id.Replace(".prefab", string.Empty);

            var prefab = Resources.Load(databaseFolder + id) as GameObject;
            var entity = prefab.GetComponent<Entity>();
            if(!entity)
            {
                Debug.LogError("Database: GameObject without entity found, skipping.", prefab);
                continue;
            }


            prefab.GetComponent<Entity>().database_ID = id;
        }
    }

    static void assign_ids_components()
    {
        

        var allData = Resources.LoadAll("Database/");
        prefabObjects = new List<GameObject>(1000);
        foreach (var item in allData)
        {
            GameObject go = item as GameObject;
            var entity = go.GetComponent<Entity>();
            if(!entity)
            {
                Debug.LogError("Database: GameObject without entity found, skipping.", go);
                continue;
            }

            prefabObjects.Add(go);
            entities.Add(entity.database_ID, entity);

            ComponentBase[] components = go.GetComponentsInChildren<ComponentBase>();
            HashSet<string> comps_in_entity = null;
            Components.TryGetValue(entity.database_ID, out comps_in_entity);

            if (comps_in_entity == null)
                comps_in_entity = new HashSet<string>();

            foreach (var c in components)
            {
                if(c.ID == string.Empty || c.ID == null)
                {
                    c.ID = get_unique_id(comps_in_entity);
                }
                comps_in_entity.Add(c.ID);
            }
        }
    }


    static string get_unique_id(HashSet<string> set)
    {
        Guid guid = Guid.NewGuid();
        string id = guid.ToString();
        if (set.Contains(id))
            return get_unique_id(set);
        else
            return id;
    }
}
