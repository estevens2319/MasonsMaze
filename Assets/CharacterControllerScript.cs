using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterControllerScript : MonoBehaviour {

    private Animator animation_controller;
    private CharacterController character_controller;
    public Vector3 movement_direction;
    public float walking_velocity;
    public Text text;    
    public float velocity;
    public int num_lives;
    public bool has_won;
    public float turnSpeed;
    public int currAction;
    public float acceleration;
    public bool isDead;
    public Button resetButton;
    public GameObject dreyar;
    public GameObject erika;
    public bool erikaSelected;
	// Use this for initialization
	void Start ()
    {
        animation_controller = GetComponent<Animator>();
        character_controller = GetComponent<CharacterController>();
        movement_direction = new Vector3(0.0f, 0.0f, 0.0f);
        walking_velocity = .25f;
        velocity = 0.0f;
        num_lives = 5;
        has_won = false;
        turnSpeed = 200.0f;
        currAction = 0;
        acceleration = 0.005f;
        isDead = false;
        resetButton.gameObject.SetActive(false);
        if(erikaSelected){
        dreyar.SetActive(false);
        erika.SetActive(true);
        }
        else{
        dreyar.SetActive(true);
        erika.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(has_won){
            animation_controller.SetBool("idle", true);
            animation_controller.SetBool("walkForwards", false);
            animation_controller.SetBool("crouchForwards", false);
            animation_controller.SetBool("runForwards", false);
            animation_controller.SetBool("jump", false);
            animation_controller.SetBool("walkBackwards", false);
            animation_controller.SetBool("crouchBackwards", false);
            velocity = 0.0f;
            Vector3 stop = movement_direction * velocity * Time.deltaTime;
            character_controller.Move(stop);
            resetButton.gameObject.SetActive(true);

        }
        else if(isDead){
            velocity = 0.0f;
            Vector3 stop = movement_direction * velocity * Time.deltaTime;
            character_controller.Move(stop);
            resetButton.gameObject.SetActive(true);
        }
        else if(!isDead && !has_won){
        text.text = "Lives left: " + num_lives;
        if(num_lives <=0){
            text.text = "You lost";
            animation_controller.SetTrigger("death");
            isDead = true;
        }
        ////////////////////////////////////////////////
        // WRITE CODE HERE:
        // (a) control the animation controller (animator) based on the keyboard input. Adjust also its velocity and moving direction. 
        // (b) orient (i.e., rotate) your character with left/right arrow [do not change the character's orientation while jumping]
        // (c) check if the character is out of lives, call the "death" state, let the animation play, and restart the game
        // (d) check if the character reached the target (display the message "you won", freeze the character (idle state), provide an option to restart the game
        // feel free to add more fields in the class        
        ////////////////////////////////////////////////
        bool upArrow = Input.GetKey(KeyCode.UpArrow);
        bool downArrow = Input.GetKey(KeyCode.DownArrow);
        bool ctrl = Input.GetKey(KeyCode.LeftControl);
        bool shift = Input.GetKey(KeyCode.LeftShift);
        bool space = Input.GetKey(KeyCode.Space);
        bool left = Input.GetKey(KeyCode.LeftArrow);
        bool right = Input.GetKey(KeyCode.RightArrow);
        currAction = 0;
        animation_controller.SetBool("walkForwards", false);
        animation_controller.SetBool("walkBackwards", false);
        animation_controller.SetBool("crouchForwards", false);
        animation_controller.SetBool("crouchBackwards", false);
        animation_controller.SetBool("runForwards", false);
        animation_controller.SetBool("jump", false);
        animation_controller.SetBool("idle", false);
        animation_controller.SetBool("runBackwards", false);

        if(space){
            animation_controller.SetBool("jump", true);
            currAction = 6;
        }
        else if(upArrow && ctrl){
            // animation_controller.SetBool("walkForwards", false);
            animation_controller.SetBool("crouchForwards", true);
            currAction = 3;
        }
        else if(upArrow && shift){
            animation_controller.SetBool("runForwards", true);
            currAction = 2;
            // animation_controller.SetBool("jump", false);
            // animation_controller.SetBool("walkForwards", false);
        }
        else if(upArrow){
            // animation_controller.SetBool("jump", false);
            // animation_controller.SetBool("crouchForwards", false);
            // animation_controller.SetBool("runForwards", false);
            animation_controller.SetBool("walkForwards", true);
            currAction = 1;
        }
        else if(downArrow && ctrl){
            // animation_controller.SetBool("walkBackwards", false);
            animation_controller.SetBool("crouchBackwards", true);
            currAction = 5;
        }
        else if(downArrow && shift){
            // animation_controller.SetBool("walkBackwards", false);
            animation_controller.SetBool("runBackwards", true);
            currAction = 7;
        }
        else if(downArrow){
            // animation_controller.SetBool("crouchBackwards", false);
            animation_controller.SetBool("walkBackwards", true);
            currAction = 4;
        }
        else{
            animation_controller.SetBool("idle", true);
            currAction = 0;
        }
        
        switch(currAction){
            case 0:
                velocity = 0.0f;
                break;
            case 1:
                if(velocity < walking_velocity){
                    velocity += acceleration;
                }
                if(velocity > walking_velocity){
                    velocity -= acceleration;
                }
                Vector3 walkForward = movement_direction * velocity * Time.deltaTime;
                Debug.Log(walkForward.ToString());
                character_controller.Move(walkForward);
                break;
            case 2:
                if(velocity < (2.0f * walking_velocity)){
                    velocity += acceleration;
                }
                if(velocity > (2.0f * walking_velocity)){
                    velocity -= acceleration;
                }
                Vector3 runForward = movement_direction * velocity * Time.deltaTime;
                character_controller.Move(runForward);
                break;
            case 3:
                if(velocity <= (1.0f * walking_velocity / 2.0f)){
                    velocity += acceleration;
                }
                if(velocity > (1.0f * walking_velocity / 2.0f)){
                    velocity -= acceleration;
                }
                Vector3 crouchForward = movement_direction * velocity * Time.deltaTime;
                character_controller.Move(crouchForward);
                break;
            case 4:
                if(velocity < (-1.0f * walking_velocity / 1.5f)){
                    velocity += acceleration;
                }
                if(velocity > (-1.0f * walking_velocity / 1.5f)){
                    velocity -= acceleration;
                }
                Vector3 walkBackward = movement_direction * velocity * Time.deltaTime;
                character_controller.Move(walkBackward);
                break;
            case 5:
                if(velocity < (-1.0f * walking_velocity / 2.0f)){
                    velocity += acceleration;
                }
                if(velocity > (-1.0f * walking_velocity / 2.0f)){
                    velocity -= acceleration;
                }
                Vector3 crouchBackward = movement_direction * velocity * Time.deltaTime;
                character_controller.Move(crouchBackward);
                break;
            case 6:
                // if(velocity < (2.0f * walking_velocity)){
                //     velocity += acceleration;
                // }
                // if(velocity > (2.0f * walking_velocity)){
                //     velocity -= acceleration;
                // }
                Vector3 jumpForward = movement_direction * velocity * Time.deltaTime;
                character_controller.Move(jumpForward);
                break;
            case 7:
                if(velocity < (-2.0f * walking_velocity)){
                    velocity += acceleration;
                }
                if(velocity > (-2.0f * walking_velocity)){
                    velocity -= acceleration;
                }
                Vector3 runBackward = movement_direction * velocity * Time.deltaTime;
                character_controller.Move(runBackward);
                break;
        }

        if(right){
            Vector3 rotation = new Vector3(0, turnSpeed * Time.deltaTime, 0);
            this.transform.Rotate(rotation);
        }
        if(left){
            Vector3 rotation = new Vector3(0, (-1 * turnSpeed * Time.deltaTime), 0);
            this.transform.Rotate(rotation);
        }

        }
        Vector3 curr = this.transform.position;
        // curr.y = 0;
        this.transform.position = curr;
        // Debug.Log(velocity);
        // you don't need to change the code below (yet, it's better if you understand it). Name your FSM states according to the names below (or change both).
        // do not delete this. It's useful to shift the capsule (used for collision detection) downwards. 
        // The capsule is also used from turrets to observe, aim and shoot (see Turret.cs)
        // If the character is crouching, then she evades detection. 
        bool is_crouching = false;
        if ( (animation_controller.GetCurrentAnimatorStateInfo(0).IsName("CrouchForward"))
         ||  (animation_controller.GetCurrentAnimatorStateInfo(0).IsName("CrouchBackward")) )
        {
            is_crouching = true;
        }

        if (is_crouching)
        {
            GetComponent<CapsuleCollider>().center = new Vector3(GetComponent<CapsuleCollider>().center.x, 0.0f, GetComponent<CapsuleCollider>().center.z);
        }
        else
        {
            GetComponent<CapsuleCollider>().center = new Vector3(GetComponent<CapsuleCollider>().center.x, 0.9f, GetComponent<CapsuleCollider>().center.z);
        }

        // you will use the movement direction and velocity in Turret.cs for deflection shooting 
        float xdirection = Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.y);
        float zdirection = Mathf.Cos(Mathf.Deg2Rad * transform.rotation.eulerAngles.y);
        movement_direction = new Vector3(xdirection, 0.0f, zdirection);

        // character controller's move function is useful to prevent the character passing through the terrain
        // (changing transform's position does not make these checks)
        if (transform.position.y > 0.0f) // if the character starts "climbing" the terrain, drop her down
        {
            Vector3 lower_character = movement_direction * velocity * Time.deltaTime;
            lower_character.y = -100f; // hack to force her down
            character_controller.Move(lower_character);
        }
        else
        {
            character_controller.Move(movement_direction * velocity * Time.deltaTime);
        }
    }                    
}
