var dbserver, dbname, dbuser, dbpwd, schema, storageaccount
function migrateData() {
    dbserver = $("#dbserver").val();
    dbname = $("#dbname").val();
    dbuser = $("#dbuser").val();
    dbpwd = $("#dbpwd").val();
    schema = $("#schema").val();
    storageaccount = $("#storageaccount").val();
    if (validateUser()) {
        //console.log(dbserver + " " + dbname + " " + dbuser + " " + dbpwd + " " + schema + " " + storageaccount)
        var data = {
            dbserver: dbserver,
            dbname: dbname,
            dbuser: dbuser,
            dbpwd: dbpwd,
            schema: schema,
            storageaccount: storageaccount
        }
        $.ajax({
            type: "POST",
            url: "/Home/JobTrigger",
            dataType: "json",
            async: true,
            data: data,
            beforeSend: function () {
                console.log("before running");
                $("#loader").show();
                $("#migratebtn").hide();
            },
            success: function (response) {
                console.log(response);
                swal("Success!", response.res, "success");
                window.location.href = "/Home/ViewResult";
            },
            complete: function () {
                console.log("After running");
                $("#loader").hide();
            },
            error: function (req, status, error) {
                alert(status);
            }
        });
    }
}

function validateUser() {
    if (dbserver === "" || dbname === "" || dbuser === "" || dbpwd === "" || schema === "" || storageaccount === "") {
        swal("Error!", "All fields are mandatory!", "error");
        return false;
    }
    else
        return true;
}

function showhome() {
    window.location.href = "/Home/IndexPage";
}

function getSchema() {
    if ($('.schema').length) {
        $('.schema').find('option').remove().end().append('<option value="whatever"></option>').val('whatever');
    }
    $('#source-data-window').empty();
    $('#app-ci').hide();
    $('#appCIName').val('');
    $('#migratebtn').hide();
    $('#valid-app-ci').hide();
    $('#invalid-app-ci').hide();
    DbType = "sqlserver";
    DbServer = $("#dbserver").val();
    DbName = $("#dbname").val();
    DbUsername = $("#dbuser").val();
    DbPassword = $("#dbpwd").val();

    if (validateInput()){
        $.ajax({
            url: "/Home/GetSchema",
            type: "get",
            data: {
                DbType: DbType,
                DbServer: DbServer,
                DbName: DbName,
                DbUsername: DbUsername,
                DbPassword: DbPassword
            },
            beforeSend: function () {
                console.log("before running");
                $("#loader").show();
                $(".error-message").hide();
                $(".schema").hide();
                
            },
            success: function (response) {
                console.log(response);
                if (response.toString() == "Loginfailed") {
                    $(".error-message").html("Please enter correct username or password");
                    $(".error-message").css("display", "block");
                }
                else if (response.toString() == "InvalidServer") {
                    $(".error-message").html("Please enter correct database server name");
                    $(".error-message").css("display", "block");
                }
                else {
                    $(".schema").show();
                    schemaList = JSON.parse(response);
                    $('#schema').append($("<option></option>").attr("value", "Blank").text("Select Schema"));
                    $.each(schemaList, function (key, value) {
                        $('#schema')
                            .append($("<option></option>")
                                .attr("value", value)
                                .text(value));
                    });
                    //console.log(schemaList);
                }
            },
            complete: function () {
                console.log("After running");
                $("#loader").hide();
            },
            error: function (xhr) {
                console.log("error:" + xhr.toString());
                window.location.href = "~/Views/Shared/Error.cshtml";
            }
        });
    }
}

function validateInput() {
    if (DbServer == "" || DbUsername == "" || DbPassword == "" || DbName == "") {
        $(".error-message").html("All fields are mandatory");
        $(".error-message").css("display", "block");
        return false;
    }
    else
        return true;
}

function showTables() {
    var schemaName = $('#schema').find(":selected").text();
    $('#source-data-window').show();
    $.ajax({
        url: "/Home/GetTables",
        type: "get",
        data: {
            DbType: DbType,
            DbServer: DbServer,
            DbName: DbName,
            DbUsername: DbUsername,
            DbPassword: DbPassword,
            schema: schemaName
        },
        beforeSend: function () {
            console.log("before running");
            //$("#loader").show();
            //$(".error-message").hide();
            //$(".schema").hide();
            $('#source-data-window').append('<i id="table-loader" class="fa fa-spinner fa-spin fa-2x"></i>')
        },
        success: function (response) {
            addTable(response, schemaName);
            console.log(response);
            if (schemaName != "Select Schema") {
                $("#app-ci").show();
                $("#migratebtn").show();
            }
        },
        complete: function () {
            console.log("After running");
            //$("#loader").hide();
            $('#table-loader').remove();
        },
        error: function (xhr) {
            console.log("error:" + xhr.toString());
            window.location.href = "~/Views/Shared/Error.cshtml";
        }
    });
}


function addTable(data, schema) {
    $(document).ready(function () {
        //Adding table list for given schema if does not already exist
        if ($(`#${schema}`).length || schema=="Select Schema")
            return;
        $('#source-data-window').append(
            $('<div>').prop({
                id: `${schema}`,
                class: 'schema-window',
                innerHTML: schema.toString()
            })
        );
        $(`#${schema}`).append(
            `<button id="closebtn" class="${schema}" onclick="closeSchema()">
               <i class="fa-solid fa-square-xmark">
            </button>`
        );
        $(`#${schema}`).append(
            $('<div>').prop({  
                id: `table-window-${schema}`,
                class: 'schema-table-window'         
            })
        );
        $(`#table-window-${schema}`).append(
            $('<table>').prop({ id: `table-list-${schema}`, class: 'table-list'})
        );
        $(`#table-list-${schema}`).append(
            `<thead id = "thead-${schema}"></thead>`
        );
        $(`#table-list-${schema}`).append(
            `<tbody id = "tbody-${schema}"></tbody>`
        );
        $(`#thead-${schema}`).append(
            `<tr><th><input type="checkbox" id="checkboxAll-${schema}"></th><th>Table Name</th></tr>`
        );

        tableList = JSON.parse(data);
        $.each(tableList, function (key, value) {
            $(`#tbody-${schema}`).append(
                `<tr><td><input type="checkbox" class="checkbox-${schema}" name="checkbox-${value}" value="${value}"></td><td>${value}</td></tr>`
            );
        });
        /*$(`#${schema}`).append([a
            $('<label>').prop({ for: `app-ci-${schema}`, innerHTML: 'APP CI Name' }),
            $('<input>').prop({ type: 'search', id: `app-ci-${schema}`, name: `app-ci-${schema}`, css: 'width:5px' }),
            $('<button>').prop({ id: `app-ci-btn-${schema}`, innerHTML: 'Validate', onclick: 'checkContainer()' }),
        ]);*/
        $(`#app-ci-btn-${schema}`).click({schemaName: schema}, checkContainer);

        $(`#checkboxAll-${schema}`).click(function () {
            if ($(`#checkboxAll-${schema}`).is(':checked')) {
                //console.log("Select All");
                $(`.checkbox-${schema}`).prop('checked', true);
            } else {
                //console.log("Deselect all");
                $(`.checkbox-${schema}`).prop('checked', false);
            }
        });

    });
}

function checkContainer() {
    var containerName = $('#appCIName').val();
    if (containerName === "") {
        swal("Error!", "App CI Name can not be empty!", "error");
        return;
    }
    console.log(containerName);
    $.ajax({
        url: "/Home/ContainerExist",
        type: "get",
        data: {
            containerName: containerName
        },
        beforeSend: function () {
            console.log("before running");
            //$("#loader").show();
            //$(".error-message").hide();
            //$(".schema").hide();
            $('#app-ci-loading-container').append('<i id="appci-loader" class="fa fa-spinner fa-spin fa-2x"></i>')
        },
        success: function (response) {
            console.log(response);
            if (response == "True") {
                //swal("Error!", "Container already exist", "error");
                $('#valid-app-ci').hide();
                $('#invalid-app-ci').show();
            }
            else {
                console.log(containerName);
                $('#valid-app-ci').show();
                $('#invalid-app-ci').hide();
            }
        },
        complete: function () {
            console.log("After running");
            //$("#loader").hide();
            $("#appci-loader").remove();
        },
        error: function (xhr) {
            console.log("error:" + xhr.toString());
            window.location.href = "~/Views/Shared/Error.cshtml";
        }
    });
    console.log("check container");
}

function migrate() {
    //check at least 1 table is selected
    //all app-ci-name are validated

    DbType = "sqlserver";
    DbServer = $("#dbserver").val();
    DbName = $("#dbname").val();
    DbUsername = $("#dbuser").val();
    DbPassword = $("#dbpwd").val();
    ContainerName = $('#appCIName').val();
    StrorageAccount = "laptestblob1";

    var dataList = [];
    $('#source-data-window').children('div').each(function () {
        var tableList = [];
        $(this).find('td').find('input').each(function () {
            if($(this).is(':checked'))
                tableList.push(this.value);
        });
        if (tableList.length != 0)
            dataList.push({ SchemaName: this.id, TableList: tableList });

    });

    console.log(dataList);

    var checkData = false;
    dataList.forEach(function (obj) {
        if (obj.TableList.length != 0)
            checkData = true;
    });

    if (ContainerName === "" || !checkData) {
        console.log(ContainerName);
        swal("Error!", "App CI Name or Selected table can not be empty!", "error");
        return;
    }
    
    var data = {
        //DbType: 'abcd',
        //DbServer: 'pqrs',
        //DbUsername: 'xqp',
        //DbPassword: 'jsjjs',
        //DbName: 'aaaa'
        DbServer: DbServer,
        DbName: DbName,
        DbUser: DbUsername,
        DbPwd: DbPassword,
        StorageAccount: StrorageAccount,
        ContainerName: ContainerName,
        DbDataList: dataList
    }
    console.log("sending data = " + data);
    $.ajax({
        type: "POST",
        url: "/Home/JobTriggerUpdate",
        dataType: "json",
        async: true,
        data: data,
        beforeSend: function () {
            console.log("before running");
            //$("#loader").show();
            //$("#migratebtn").hide();
            $('#migrate-loading').append('<i id="migrate-loader" class="fa fa-spinner fa-spin fa-2x"></i>')
        },
        success: function (response) {
            console.log(response);
            swal("Success!", response.res, "success");
            window.location.href = "/Home/ViewResult";
        },
        complete: function () {
            console.log("After running");
            //$("#loader").hide();
            $('#migrate-loader').hide();
        },
        error: function (req, status, error) {
            alert(status);
            console.log(status + " " + error);
        }
    });
}

function closeSchema() {
    var id = $('#closebtn').attr('class');
    $(`#${id}`).remove();
    if ($('#source-data-window').is(':empty')) {
        $('#app-ci').hide();
        $('#appCIName').val('');
        $('#migratebtn').hide();
        $('#valid-app-ci').hide();
        $('#invalid-app-ci').hide();
    }
}
