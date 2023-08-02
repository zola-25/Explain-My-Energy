
  let __webpack_public_path__ = process.env["ASSETS_PATH"];

  if (!__webpack_public_path__) {
    console.error("__webpack_public_path__ = " + __webpack_public_path__);
    throw new Error("ASSETS_PATH environment variable is not set");
  }
 
 export { __webpack_public_path__ }
