using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BotTurret : MonoBehaviour
{
	[SerializeField]
	private float _visionRadius = 20;

	private Unit _unit;
	private Gun _gun;
	private GameObject _target;
	private Vector3 _direction;
	private float _distance;

	private void Start()
	{
		this._unit = this.GetComponent<Unit>();
		this._gun = this.GetComponent<Gun>();
	}

	private void Update()
	{
		if ( !this._target )
			this.Scan();

		else
			this.Tracking();

		if ( this._target )
		{
			this.Calculate();
			this.Rotate();
			this.Attack();
		}
	}

	private void Scan ()
	{
		var targetObjects = this.ScopeCheck();
		foreach ( var targetObject in targetObjects )
		{
			var target = targetObject.GetComponent<Unit>();
			if ( target && target.fraction != this._unit.fraction )
				this._target = targetObject;
		}
	}

	private void Tracking ()
	{
		var targetObjects = this.ScopeCheck();
		var inRange = targetObjects.Find( item => item == this._target );
		if ( !inRange || !this._target.GetComponent<Unit>() ) this._target = null;
	}

	private void Calculate ()
	{
		this._distance = Vector3.Distance( this.transform.position, this._target.transform.position );

		var bulletVelocity = this._gun.bullet.GetComponent<Bullet>().force / 10;
		var timeDistance = this._distance / bulletVelocity;
		var targetPosition = this._target.transform.position;
		var targetVelocity = this._target.GetComponent<Rigidbody>().velocity;
		var aimPoint = targetPosition + targetVelocity * timeDistance;

		this._direction = aimPoint - this.transform.position;
	}

	private void Rotate ()
	{
		this._unit.Rotate( this._direction );
	}

	private void Attack ()
	{
		if ( this._gun && this._distance < this._visionRadius && Vector3.Angle( this.transform.forward, this._direction ) < 5f )
			this._gun.Shot();
	}

	private List<UnityEngine.GameObject> ScopeCheck ()
	{
		// Getting of all colliders that overlapping sphere
		return Physics.OverlapSphere( this.transform.position, this._visionRadius, 1 << 3 )
			// Filtering out colliders that are related to current game object
			.Where( hitCollider => hitCollider.gameObject.transform.parent != this.transform )
			// Getting colliders game objects
			.Select( hitCollider => hitCollider.gameObject.transform.parent.gameObject )
			// Converting the result to List
			.ToList();
	}

	private void OnDrawGizmos()
	{
		if ( this._target )
		{
			var bulletVelocity = 40f;
			var timeDistance = this._distance / bulletVelocity;
			var targetPosition = this._target.transform.position;
			var targetVelocity = this._target.GetComponent<Rigidbody>().velocity;
			var aimPoint = targetPosition + targetVelocity * timeDistance;

			Gizmos.color = Color.yellow;
			Gizmos.DrawRay( this.transform.position, this._target.transform.position - this.transform.position );
			Gizmos.DrawWireSphere( targetPosition, 1.5f );

			Gizmos.color = Color.gray;
			Gizmos.DrawRay( targetPosition, targetVelocity );

			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere( aimPoint, 1.5f );
			Gizmos.DrawRay( this.transform.position, aimPoint - this.transform.position );
		}
	}
}
