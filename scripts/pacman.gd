extends CharacterBody2D


const SPEED = 50.0
var on_start = 1;
@onready var animated_sprite = $Sprite
func _physics_process(delta: float) -> void:
	if on_start == 1:
		velocity.x = SPEED;
		on_start = 0;
	# Move up
	if Input.is_action_just_pressed("move_up"):
		animated_sprite.play("face_up");
		velocity.y = -SPEED
		velocity.x=0
	# Move down
	if Input.is_action_just_pressed("move_down"):
		animated_sprite.play("face_down");
		velocity.y = SPEED;
		velocity.x = 0;
		

	# Flip the sprite
	#var direction := Input.get_axis("move_left", "move_right")
	if Input.is_action_just_pressed("move_left"):
		animated_sprite.play("face_left");
		velocity.x = -SPEED;
		velocity.y = 0;
	elif Input.is_action_just_pressed("move_right"):
		animated_sprite.play("face_right");
		velocity.x = SPEED;
		velocity.y = 0;

	move_and_slide()
