using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementType
{
    PingPong,
    Loop,
}

/// <summary>
/// Moving obstacles follow a track by which they move around on
/// </summary>
public class MovingObstacle : MonoBehaviour
{
    /// <summary>
    /// Controls the movement type
    /// </summary>
    [SerializeField]
    MovementType m_movementType;

    /// <summary>
    /// Scale of the node
    /// </summary>
    [SerializeField, Range(0.1f, 1f)]
    float m_nodeSize = 0.25f;

    /// <summary>
    /// How fast the object moves
    /// </summary>
    [SerializeField]
    float m_moveSpeed = 2f;

    /// <summary>
    /// How long to wait on each node before moving to the next
    /// </summary>
    [SerializeField]
    float m_nodeChangeDelay = 0.5f;

    /// <summary>
    /// How close it needs to be to the target before considering it "reached"
    /// </summary>
    [SerializeField]
    float m_distanceToTarget = 0.001f;

    /// <summary>
    /// The parent transform that holds all the nodes this object will follow
    /// </summary>
    [SerializeField]
    Transform m_trackTransform;

    /// <summary>
    /// The nodes for this object to navigate on
    /// Using transforms in the event the track is dynamic
    /// </summary>
    List<Transform> m_track;

    /// <summary>
    /// Where on the list the player is 
    /// Starts at -1 since we add one on the GetNext call
    /// </summary>
    int m_currentIndex = -1;

    /// <summary>
    /// True once the object is moving
    /// </summary>
    bool m_movementTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        Init();        
    }

    /// <summary>
    /// Initializes movement after level is loaded
    /// </summary>
    private void Update()
    {
        if (!m_movementTriggered && GameManager.instance.IsLevelLoaded)
        {
            m_movementTriggered = true;
            StartCoroutine(MoveRoutine());
        }
    }

    /// <summary>
    /// Initializes the moving object and defines the track
    /// </summary>
    void Init()
    {
        m_track = new List<Transform>();

        if (m_trackTransform != null)
        {
            for (int i = 0; i < m_trackTransform.childCount; i++)
            {
                // Ensures the object always starts on the first node
                if (i == 0)
                {
                    transform.position = m_trackTransform.GetChild(i).position;
                }

                m_track.Add(m_trackTransform.GetChild(i));
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Show track nodes
    /// </summary>
    void OnDrawGizmos()
    {
        Init();

        if (m_track == null)
        {
            return;
        }

        for (int i = 0; i < m_track.Count; i++)
        {
            Transform current = m_track[i];
            Transform next = i + 1 < m_track.Count ? m_track[i+1] : null;

            if(next == null && m_movementType == MovementType.Loop)
            {
                next = m_track[0];
            }

            Gizmos.DrawCube(current.position, Vector3.one * m_nodeSize);

            // Draw a line to connect to the next node
            if(next != null && next != current)
            {
                Gizmos.DrawLine(current.position, next.position);
            }
        }
    }
#endif

    IEnumerator MoveRoutine()
    {
        // Reset index
        m_currentIndex = -1;

        while (m_track.Count > 0)
        {
            Transform target = GetNextTarget();

            while(Vector3.Distance(transform.position, target.position) > m_distanceToTarget)
            {
                yield return new WaitForEndOfFrame();
                transform.position = Vector3.MoveTowards(transform.position, 
                                                         target.position, 
                                                         m_moveSpeed * Time.deltaTime);
            }

            transform.position = target.position;
            yield return new WaitForSeconds(m_nodeChangeDelay);
        }
    }

    /// <summary>
    /// Returns the next target to move towards
    /// </summary>
    /// <returns></returns>
    Transform GetNextTarget()
    {
        m_currentIndex++;

        if (m_currentIndex >= m_track.Count) {
            // Resest the index but also update the list to match animation
            m_currentIndex = 0;

            // Reverse the list to go from end to start
            if(m_movementType == MovementType.PingPong)
            {
                m_track.Reverse();
            }
        }

        return m_track[m_currentIndex];
    }
}
