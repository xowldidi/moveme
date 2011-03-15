#include <stdlib.h>
#include <stdio.h>
#include <dlfcn.h>
#include <unistd.h>
#include "moveclient.h"



// Prototypes
int updateSuccess(MoveServerPacket *);
int updateFailure(int);
int updateCameraSuccess(MoveServerCameraFrameSlicePacket *);
int updateCameraFailure(int);


int main(int argc, char **argv) {
  void *lib_move;
  char *error;
  int res;

  int (*fnServerConnect)(const char *, const char *, MoveStateDeferred *);
  int (*fnServerDisconnect)();
  int (*fnPause)(void);
  int (*fnResume)(void);

  MoveStateDeferred *move_state_deferred;
  MoveServerPacket *move_server_packet;
  MoveServerCameraFrameSlicePacket *move_server_camera_frame_slice_packet;

  move_state_deferred = (MoveStateDeferred *)malloc(sizeof(MoveStateDeferred));
  move_server_packet = (MoveServerPacket *)malloc(sizeof(MoveServerPacket));
  move_server_camera_frame_slice_packet = (MoveServerCameraFrameSlicePacket *)malloc(sizeof(MoveServerCameraFrameSlicePacket));

  move_state_deferred->update_success = (void *)&updateSuccess;
  move_state_deferred->update_failure = (void *)&updateFailure;
  move_state_deferred->update_camera_success = (void *)&updateCameraSuccess;
  move_state_deferred->update_camera_failure = (void *)&updateCameraFailure;
  move_state_deferred->move_server_packet = move_server_packet;
  move_state_deferred->move_server_camera_frame_slice_packet = move_server_camera_frame_slice_packet;


  // Open shared library
  lib_move = dlopen("libmove.so", RTLD_LOCAL|RTLD_LAZY);

  if (!lib_move) {
    fprintf(stderr, "%s\n", dlerror());
    exit(1);
  }

  // Lookup functions
  fnServerConnect = dlsym(lib_move, "serverConnect");

  if ((error = dlerror()) != NULL) {
    fprintf(stderr, "ServerConnect error: %s\n", error);
    exit(1);
  }

  fnServerDisconnect = dlsym(lib_move, "serverDisconnect");

  if ((error = dlerror()) != NULL) {
    fprintf(stderr, "ServerDisconnect error: %s\n", error);
    exit(1);
  }

  fnPause = dlsym(lib_move, "pause");

  if ((error = dlerror()) != NULL) {
    fprintf(stderr, "Pause error: %s\n", error);
    exit(1);

  }

  fnResume = dlsym(lib_move, "resume");

  if ((error = dlerror()) != NULL) {
    fprintf(stderr, "Resume error: %s\n", error);
    exit(1);

  }


  // Connect
  res = (*fnServerConnect)("10.98.35.189", "7899", move_state_deferred);
  fprintf(stdout, "%d\n", res);

  sleep(10);

  // Pause
  res = (*fnPause)();
  fprintf(stdout, "%d\n", res);

  sleep(10);

  // Resume
  res = (*fnResume)();
  fprintf(stdout, "%d\n", res);

  sleep(10);

  // Disconnect
  res = (*fnServerDisconnect)();
  fprintf(stdout, "%d\n", res);


  // Close shared library
  dlclose(lib_move);

  // Free heap
  free(move_state_deferred);
  free(move_server_packet);
  free(move_server_camera_frame_slice_packet);

  return 0;
}


int updateSuccess(MoveServerPacket *move_server_packet) {
  fprintf(stderr, "%f, %f, %f, %f\n", move_server_packet->state[0].pos[0], move_server_packet->state[0].pos[1], move_server_packet->state[0].pos[2], move_server_packet->state[0].pos[3]);

  return 0;
}


int updateFailure(int error) {
  fprintf(stderr, "Error: %d\n", error);

  return 0;
}


int updateCameraSuccess(MoveServerCameraFrameSlicePacket *move_server_camera_frame_slice_packet) {
  fprintf(stderr, "Camera Frame\n");

  return 0;
}


int updateCameraFailure(int error) {
  fprintf(stderr, "Camera Error: %d\n", error);

  return 0;
}


