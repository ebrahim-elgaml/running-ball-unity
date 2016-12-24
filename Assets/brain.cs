using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class brain : MonoBehaviour {
	public float speed = 6.0F;
	public float jumpSpeed = 20.0F;
	public float gravity = 20.0F;
	public float moveSpeed = 3.0F;
	public float forwardSpeed = 3.0f;
	private Vector3 moveDirection = Vector3.zero;
	bool isPaused = false;
	CharacterController controller;
	int score;
	public Text myText;
	int maxScore;
	public int prefabFactor = 200;
	public Material red;
	public Material blue;
	public Material grey;
	public Material green;
	ArrayList materials;
	// Use this for initialization
	int prefabZ = 200;
	ArrayList lanes;
	Renderer sphere;
	Transform currentMain;
	public Button resume;
	public Button restart;
	public Button quit;
	bool  isLose = false;
	bool isMute = false;
	bool isJumping = false;

	public AudioClip rolling;
	public AudioClip jump;
	public AudioClip land;
	public AudioClip changeColor;
	public AudioClip collectYellow;
	public AudioClip collectPurple;
	public AudioClip click;
	public AudioClip matchSound;
	public AudioClip notMatchSound;
	public AudioClip loseSound;
	public AudioClip mainMeunPauseSound;
	public AudioClip specialMode;






	private AudioSource source;

	void Start () {
		Time.timeScale = 1;
		isMute = PlayerPrefs.GetInt ("mute") == 1;
		if (isMute) {
			source.mute = true;
		}
		score = 0;
		prefabZ = 200;
		lanes = new ArrayList ();
		materials = new ArrayList ();
		materials.Add (red);
		materials.Add (blue);
		materials.Add (green);
		materials.Add (grey);
		controller = GetComponent<CharacterController>();
		setScore (true);
		sphere = GetComponent<Renderer>();
		restart.gameObject.SetActive (false);
		restart.onClick.AddListener(() => onRestart());
		resume.gameObject.SetActive (false);
		resume.onClick.AddListener(() => onResume());
		quit.gameObject.SetActive (false);
		quit.onClick.AddListener(() => onQuit());

		playSound (rolling);
	}

	void Awake () {
		source = GetComponent<AudioSource>();
	}

	// Update is called once per frame
	void Update () {
		checkLose ();
		if (speed <= 50) {
			speed += Time.deltaTime;
		}

		transform.position += Vector3.forward * Time.deltaTime * speed;
		if (controller.isGrounded) {
			moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			moveDirection = transform.TransformDirection(moveDirection);
			moveDirection *= speed;
			if (isJumping) {
				isJumping = false;
				playSound (land);
			}
			if (Input.GetKeyDown (KeyCode.Space)) {
				moveDirection.y = jumpSpeed;
				playSound (jump);
				isJumping = true;
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow) ||Input.GetKeyDown(KeyCode.A) )
				moveDirection.x = moveSpeed;
			if (Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.D) )
				moveDirection.x = -1 * moveSpeed;

		}
		moveDirection.y -= gravity * Time.deltaTime;
		controller.Move(moveDirection * Time.deltaTime);
		if (Input.GetKeyDown (KeyCode.Q)) {
			changeSphereColor (0);
		}
		if (Input.GetKeyDown (KeyCode.W)) {
			changeSphereColor (1);
		}
		if (Input.GetKeyDown (KeyCode.E)) {
			changeSphereColor (2);
		}
		if (Input.GetKeyDown (KeyCode.Escape) && !isPaused) {
			pause ();
		}
	}
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.name.StartsWith ("YellowCube")) {
			setScore (true);
			Destroy (other.gameObject);
			playSound (collectYellow);
		}
		if (other.gameObject.name.StartsWith ("PurpleCube")) {
			other.transform.parent.transform.parent.GetChild(0).GetComponent<Renderer> ().material = grey;
			Destroy (other.gameObject);
			playSound (collectPurple);
			playSound (specialMode);
		}
		if (other.gameObject.name.StartsWith ("Detector")) {
			currentMain = other.transform.parent;
			if (currentMain.GetChild (0).GetComponent<Renderer> ().material.color != grey.color) {
				if (currentMain.GetChild (0).GetComponent<Renderer> ().material.color != sphere.material.color) {
					setScore (false);
					playSound (notMatchSound);
				} else {
					playSound (matchSound);
				}
				
			}
			GameObject instance = Instantiate(Resources.Load("Main", typeof(GameObject)),new Vector3(0, 0, prefabZ),Quaternion.identity) as GameObject;
			if (instance == null) {
				print ("NUL INSTANCE ");
			}
			int rand = Random.Range (0, 4);
			instance.transform.GetChild (0).GetComponent<Renderer> ().material = (Material)materials [rand];
			prefabZ += prefabFactor;
			lanes.Add (instance);
			if (lanes.Count > 5) {
				Destroy ((GameObject)lanes [0]);
				lanes.RemoveAt (0);
			}
			Transform yellowCubes = instance.transform.GetChild (3);
			for(int i = 0; i < Random.Range (0, yellowCubes.childCount) ; i++){
				Destroy( yellowCubes.GetChild (i).gameObject);
			}
			Transform purbleCubes = instance.transform.GetChild (4);
			int purbleRandom = Random.Range (0, 10);
			if (purbleRandom < 3) {
				Destroy(purbleCubes.GetChild(1).gameObject);
			} else if (purbleRandom < 8) {
				Destroy(purbleCubes.GetChild(0).gameObject);
				Destroy(purbleCubes.GetChild(1).gameObject);
			}
		}
	}

	void setScore(bool inc) {
		if (inc) {
			score += 20;
			if (score > maxScore) {
				maxScore = score;
			}
		} else {
			score = score / 2;
		}
		myText.text = "Score : " + score.ToString ();
	}

	void checkLose() {
		if (score <= 0) {
			playSound (loseSound);
			myText.text = "Loser!!! Your high score is " + maxScore.ToString ();
			Time.timeScale = 0;
			isPaused = true;
			isLose = true;
			quit.gameObject.SetActive (true);
			resume.gameObject.SetActive (true);
			restart.gameObject.SetActive (true);

		}
	}
	void changeSphereColor(int i) {
		if (isPaused)
			return;
		if (!currentMain)
			return;
		sphere.material = (Material) materials [i];
		currentMain.GetChild (1).GetChild (0).GetComponent<Light> ().color = sphere.material.color;
		currentMain.GetChild (2).GetChild (0).GetComponent<Light> ().color = sphere.material.color;
		if (currentMain.GetChild (0).GetComponent<Renderer> ().material.color != grey.color) {
			if (currentMain.GetChild (0).GetComponent<Renderer> ().material.color != sphere.material.color) {
				setScore (false);
				playSound (notMatchSound);
			} else {
				playSound (matchSound);
			}

		}

		playSound (changeColor);
	}
	void pause() {
		if (isPaused) {
			source.Pause ();
			source.Stop ();
			source.PlayOneShot (rolling);
			isPaused = false;
			Time.timeScale = 1;
			quit.gameObject.SetActive (false);
			resume.gameObject.SetActive (false);
			restart.gameObject.SetActive (false);
		} else {
			source.Pause ();
			source.Stop ();
			source.PlayOneShot (mainMeunPauseSound);
			Time.timeScale = 0;
			isPaused = true;
			quit.gameObject.SetActive (true);
			resume.gameObject.SetActive (true);
		}
	}
	void onRestart() {
		playSound (click);
		isPaused = false;
		Time.timeScale = 1;
		quit.gameObject.SetActive (false);
		resume.gameObject.SetActive (false);
		SceneManager.LoadScene ("GameScene", LoadSceneMode.Single);
	}
	void onResume(){
		playSound (click);
		if (isLose) {
			isLose = false;
			score = maxScore;
		}
		if (isPaused) {
			pause ();
		}

	}
	void onQuit() {
		playSound (click);
		isPaused = false;
		Time.timeScale = 1;
		quit.gameObject.SetActive (false);
		resume.gameObject.SetActive (false);
		SceneManager.LoadScene ("FirstScene", LoadSceneMode.Single);
	}
	void playSound(AudioClip c) {
		if(!isMute)
			source.PlayOneShot (c);
	}
}
