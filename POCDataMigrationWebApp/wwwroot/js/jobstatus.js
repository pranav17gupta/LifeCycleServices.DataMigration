let id;
var per = 2;
var progressBar = $('.progressbar')
async function getDet() {
    let url = "https://localhost:7205/api/Job/GetJobResult/" + runId;
    try {
        let res = await fetch(url);
        return await res.json();
    }
    catch (error) {
        console.log(error);
    }
}
async function renderDetails() {
    let resp = await getDet();
    if (per < 90)
        per += Math.floor((Math.random() * 10)/2);
    progressBar.css("width", per + "%");
    $("#percent").text(per + '%');
    console.log(resp);
    let lifeCycleState = resp.metadata.state.life_cycle_state;
    var respStatus = lifeCycleState;
    if (lifeCycleState === 'TERMINATED') {
        console.log(resp.metadata.state.result_state);
        respStatus = resp.metadata.state.result_state

        if (respStatus === 'SUCCESS') {
            console.log(`Message : ${resp.notebook_output.result}`);
            $("#res").text(resp.notebook_output.result);
            per = 100;
            progressBar.css("width", per + "%");
            $("#percent").text(per + '%');
            $("#migrating").hide();
            $("#migrated").show();
            clearInterval(id);
        }
        else if (respStatus === 'CANCELED') {
            swal("Error!", "Please try again later!", "error").then((value) => {
                window.location.href = "/Home/Index";
            });
        }
        else {
            var error = resp.error;
            var errorMessage = error.substring(0, error.indexOf(":"));
            if (errorMessage === "shaded.databricks.org.apache.hadoop.fs.azure.AzureException") {
                //Invalid storage account
                swal("Error!", "Invalid Storage Account! \n Please enter correct storage account name", "error").then((value) => {
                    window.location.href = "/Home/Index";
                });
            }
            else if (errorMessage === "com.microsoft.sqlserver.jdbc.SQLServerException") {
                //Inavlid SQL DB details
                swal("Error!", "Invalid SQL Database credentials! \n Please enter correct SQL DB credentials", "error").then((value) => {
                    window.location.href = "/Home/Index"
                });
            }
            else {
                console.log("Error : Please Try Again later");
                respStatus = 'FAILED';
                swal("Error!", "Please try again later!", "error").then((value) => {
                    window.location.href = "/Home/Index";
                });
            }
        }
    }
    else {
        console.log(lifeCycleState);
        respStatus = lifeCycleState;
    }
    if (respStatus === 'INTERNAL_ERROR') {
        $(".bar").remove();
        swal("Error!", "Cluster is not up... \n Please try again later!", "error").then((value) => {
            window.location.href = "/Home/Index";
        });
        clearInterval(id);
    }
    console.log(`Final state : ${respStatus}`);
    var finalStatus = respStatus;
    if (finalStatus !== 'SUCCESS')
        finalStatus = 'IN-PROGRESS'
    $("#status").text(finalStatus);
}
//renderDetails()
id = setInterval(renderDetails, 3000);