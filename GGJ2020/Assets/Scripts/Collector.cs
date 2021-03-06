﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Collector : MonoBehaviour
{
    [Tooltip("How close the mouse has to be to the collectable for it to be clickable")]
    public float mouseDistance = .5f;

    [Tooltip("How close this collector has to be to the collectable for it to be clickable")]
    public float collectionDistance = 2f;

    [Tooltip("Needed to calculate the mouses cursors world position")]
    public float distanceOfCameraFromGround = 10f;

    [Tooltip("Input.GetButtonUp to use for clicking")]
    public string ClickButton = "Fire1";

    [Header("Debug")]
    public Collectable lastHovering;
    public Collectable[] allItems;
    public Collectable[] lastCollectable;

    void Start()
    {
        allItems = FindObjectsOfType<Collectable>();
        foreach (var item in allItems)
        {
            item.SetClickability(Collectable.ClickabilityEnum.OutOfRange);
        }
    }

    void Update()
    {
        updateItemClickability();

        if (lastHovering != null && Input.GetButtonUp(ClickButton))
        {
            lastHovering.ClickIt();
        }
    }

    void updateItemClickability()
    {
        var everythingInCollectionRange = Physics.OverlapSphere(this.transform.position, collectionDistance)
            .Where
            (
               i => i.GetComponent<Collectable>() != null
               && isInLineOfSight(i)
            )
            .Select(i => i.GetComponent<Collectable>()).ToArray();

        // de-activate items
        var itemsNewlyOutOfRange = lastCollectable.Where(last => everythingInCollectionRange.All(now => last != now));
        foreach (var item in itemsNewlyOutOfRange)
        {
            item.SetClickability(Collectable.ClickabilityEnum.OutOfRange);
        }

        // active new items
        foreach (var collided in everythingInCollectionRange)
        {
            collided.SetClickability(Collectable.ClickabilityEnum.Clickable);
        }
        lastCollectable = everythingInCollectionRange;

        // handle nothing in collection range
        if (everythingInCollectionRange == null || everythingInCollectionRange.Count() <= 0)
            return;

        // determine players mouse cursor position in world
        var mouseInWorld = Input.mousePosition;
        mouseInWorld.z = distanceOfCameraFromGround;
        mouseInWorld = Camera.main.ScreenToWorldPoint(mouseInWorld);

        // item in everythingInCollectionRange that is closest to the mouse cursor
        var closestToMouse = everythingInCollectionRange
            .OrderBy(c => Vector3.Distance(c.transform.position, mouseInWorld))
            .First();

        // highlight closest collectable to the mouse if it's within hover range
        if (Vector3.Distance(closestToMouse.transform.position, mouseInWorld) < mouseDistance)
        {
            lastHovering = closestToMouse;
            closestToMouse.SetClickability(Collectable.ClickabilityEnum.Hovering);
        }
        else
        {
            lastHovering = null;
        }
    }

    /// <summary>
    /// Returns true if there is no obstable between the center of this object and the supplied item
    /// </summary>
    private bool isInLineOfSight(Collider item)
    {
        RaycastHit hitInfo;
        var dir = item.transform.position - transform.position;
        // Debug.DrawRay(transform.position, dir);
        if (Physics.Raycast(transform.position, dir, out hitInfo, 1000))
        {
            // make sure the object hit matches the supplied item
            return item.gameObject == hitInfo.collider.gameObject;
        }

        return false;
    }
}
