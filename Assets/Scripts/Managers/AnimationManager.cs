using UnityEngine;
using System.Collections;

public class AnimationManager : MonoBehaviour {

	//Animation
	private Animator anim;
	AnimatorStateInfo currentState;
	float playbackTime;
	private SpriteRenderer sr;

	// Use this for initialization
	void Start ()
	{
		//Get Animation
		anim = GetComponent<Animator> ();
		sr = GetComponent<SpriteRenderer> ();

	}
	
	// Update is called once per frame
	void Update () {

		//get animations
		currentState = anim.GetCurrentAnimatorStateInfo (0);
		playbackTime = currentState.normalizedTime % 1;

	}

	//Let others call animation into object
	public void CallAnimation (string animName)
	{
		anim.Play (animName, -1, 0f);	//If you call for the animation ATTACK, even if  a different player class calls it, as long as they both have the name "ATTACK" for their attack animation, this should work
	}
		
	public void setSpeed (float speed)
	{
		anim.speed = speed; //Set speed of animation
	}
		
	public float getSpeed ()
	{
		return anim.speed; //Get speed of animation
	}

	//Check if a specific animation is running
	public bool IsAnimationRunning (string animName)
	{
		return (anim.GetCurrentAnimatorStateInfo (0).IsName (animName));
	}

	//Let others get the current state of the animation
	public AnimatorStateInfo getState ()
	{
		return currentState;
	}

	//Let others get the Sprite Renderer into object
	public SpriteRenderer getSpriteRenderer ()
	{
		return sr;
	}

	//Send whole animator
	public Animator getAnimator ()
	{
		return anim;
	}

}
