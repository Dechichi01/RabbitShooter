using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomsGenerator : MonoBehaviour {

    public Module[] modules;
    public Module startModule;

    public int iterations = 5;

    public void GenerateRooms()
    {
       string holderName = "Generated Map";
        if (transform.FindChild(holderName) != null)
            DestroyImmediate(transform.FindChild(holderName).gameObject);

        Transform generatedMap = new GameObject(holderName).transform;
        generatedMap.parent = transform;


        Module startingModule = (Module)Instantiate(startModule, transform.position, transform.rotation);
        startingModule.transform.GetComponent<MapGenerator>().GenerateMap();////////--------------VER ISSO
        startingModule.transform.parent = generatedMap;
        List<Exit> pendingExits = startingModule.GetExits();

        Module currentModule = startingModule;

        for (int i = 0; i < iterations; i++)
        {
            List<Exit> newExits = new List<Exit>();

            foreach (Exit exit in pendingExits)
            {
                string newTag = exit.GetRandomConnectTag();
                Module newModulePrefab = GetRandomWithTag(modules, newTag);
                Module newModule = Instantiate(newModulePrefab);
                newModule.transform.GetComponent<MapGenerator>().GenerateMap();////////--------------VER ISSO
                List<Exit> newModuleExits = newModule.GetExits();
                Exit exitToMatch = GetAppropriateExit(newModuleExits);
                MatchExits(exit, exitToMatch);
                newExits.AddRange(newModuleExits.FindAll(e => e != exitToMatch));

                newModule.transform.parent = generatedMap;
            }

            pendingExits = newExits;
        }

        Module[] instantiatedModules = generatedMap.GetComponentsInChildren<Module>();
    }

    

    private Exit GetAppropriateExit(List<Exit> exits)
    {
        Exit exit = exits[Random.Range(0, exits.Count)];
        for (int i = 0; i < exits.Count; i++)
        {
            if (exits[i].isDefaultConnection)
            {
                exit = exits[i];
                break;
            }
        }

        return exit;
    }

    private void MatchExits(Exit oldExit, Exit newExit)
    {
        Transform newModule = newExit.Room;
        Vector3 forwardVectorToMatch = -oldExit.transform.forward;
        float angleToRotate = Azimuth(forwardVectorToMatch) - Azimuth(newExit.transform.forward);
        newModule.RotateAround(newExit.transform.position, Vector3.up, angleToRotate);
        Vector3 translation = oldExit.transform.position - newExit.transform.position;
        newModule.transform.position += translation;

        DestroyImmediate(newExit.transform.parent.gameObject);
    }

    private Module GetRandomWithTag(Module[] modules, string tagToMatch)
    {
        List<Module> matchingModules = new List<Module>();
        for (int i = 0; i < modules.Length; i++)
        {
            if (modules[i].Tag == tagToMatch)
                matchingModules.Add(modules[i]);            
        }
        return matchingModules[Random.Range(0, matchingModules.Count)];
    }

    private static float Azimuth(Vector3 vector)
    {
        return Vector3.Angle(Vector3.forward, vector) * Mathf.Sign(vector.x);
    }

}
