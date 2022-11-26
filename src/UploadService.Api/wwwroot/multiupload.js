const fileInput = document.getElementById("file");
const uploadButton = document.getElementById("upload");
const fileListctrol = document.getElementById("fileList");
const uploadAllButton = document.getElementById("uploadAll");
const uploadList=document.getElementById("uploadList");
const fileList = [];

uploadButton.addEventListener("click", () => {;
    AddFile(fileInput.files[0]);
});

//add file to the list
const AddFile = (file) => {
    fileList.push(file);
    rowTemplate(file,fileList.length-1);
}
const rowTemplate = (file,index) => { 
    var row = document.createElement("tr");
    row.innerHTML = `
                    <tr>
                    <th scope="row">${index+1}</th>
                    <td>${file.name}</td>
                    <td>${parseFloat( file.size/(1024*1024)).toFixed(2)} MB</td>
                    <td>${file.type?file.type:file.name.split('.').pop()}</td>
                    <td><button class="btn btn-danger" onclick="RemoveFile(${index})">Remove</button></td>
                </tr>`;
    fileListctrol.appendChild(row);
}
const RemoveFile = (index) => {
    fileList.splice(index, 1);
    fileListctrol.innerHTML = "";
    for (var i = 0; i < fileList.length; i++) {
        rowTemplate(fileList[i],i);
    }
}

//constants
const sessionUrl = "https://localhost:5251/upload/session";
const uploadUrl = "https://localhost:5251/upload/part";
const completeSessionUrl = "https://localhost:5251/upload/complete";
const CHUNK_SIZE = 1024 * 1024 * 2;
//methods

const createUploadSession = async (file) => {
    const chunkCount = Math.ceil(file.size / CHUNK_SIZE);
    const response = await fetch(sessionUrl, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            fileName: file.name,
            totalParts: chunkCount
        })
    });
    return await response.json();
}
const uploadFile = async (file, session, index) => {
    //progress bar
    const uploadPregress = document.getElementById("progress_" + index);;

    const chunkCount = Math.ceil(file.size / CHUNK_SIZE);
    for (var i = 0; i < chunkCount; i++)
    {
        const chunk = file.slice(i * CHUNK_SIZE, (i + 1) * CHUNK_SIZE);
        const response = await fetch(uploadUrl + `/${session}/${i + 1}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/octet-stream",
                "Content-Length": chunk.length,
            },
            body: chunk,
        });
        if(response.ok)
        {
            let progressValue=Math.round(((i+1)*chunk.size/ file.size) * 100);
            uploadPregress.style.width = progressValue+"%";
            uploadPregress.attributes["aria-valuenow"].value =progressValue;
            uploadPregress.innerHTML = progressValue + " %";
        }
    }
    //complete session
    const completeResponse = await fetch(completeSessionUrl + `/${session}`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
    });
    if (await completeResponse.ok) {
            let progressValue=100;
            uploadPregress.style.width = progressValue+"%";
            uploadPregress.attributes["aria-valuenow"].value =progressValue;
            uploadPregress.innerHTML = progressValue + " %";
    }
    else {
        alert("File upload failed");
    }
}

//upload all files
uploadAllButton.addEventListener("click",async  () => {
    //foreach file create upload row in uploadList table
    for (var i = 0; i < fileList.length; i++) {
        var row = document.createElement("tr");
        row.innerHTML = `
                    <tr>
                    <th scope="row">${i+1}</th>
                    <td>${fileList[i].name}</td>
                    <td>${parseFloat(fileList[i].size/(1024*1024)).toFixed(2)} MB</td>
                    <td>${fileList[i].type?fileList[i].type:fileList[i].name.split('.').pop()}</td>
                    <td>
                       <div class="progress">
                        <div  id="progress_${i}" class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar" style="width: 0%;" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100">0%</div>
                        </div>
                    <td>
                </tr>`;
        uploadList.appendChild(row);
    }
    //upload all files
    for (var i = 0; i < fileList.length; i++) {
         //create upload session
        await createUploadSession(fileList[i]).then(async (session) => {
            //upload file
            await uploadFile(fileList[i], session,i).then(() => {
                //alert("upload complete");
            });
        });
    }
});